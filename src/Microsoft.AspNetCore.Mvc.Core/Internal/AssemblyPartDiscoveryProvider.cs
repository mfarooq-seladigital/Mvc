// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    internal static class AssemblyPartDiscoveryProvider
    {
        public static void ConfigureApplicationParts(ApplicationPartManager partManager, string entryPointAssemblyName)
        {
            var entryAssembly = Assembly.Load(new AssemblyName(entryPointAssemblyName));
            var entryModel = AssemblyPartDiscoveryModel.ResolveEntryPoint(entryAssembly);

            foreach (var part in GetPartsInOrder(entryModel))
            {
                var operation = ConfigureApplicationPartManager.GetConfigureOperation(part);
                operation.Configure(partManager, part);
            }
        }

        // Internal for unit testing.
        internal static IEnumerable<AssemblyPartDiscoveryModel> GetPartsInOrder(AssemblyPartDiscoveryModel entryModel)
        {
            // Consider
            // App -> [ AdditionalParts: App.Views ] 
            // App.deps.json
            //  - Component1
            //  - ClassLib11 -> [ AdditionalParts: ModuleComponent, Component1 ]
            //  - ClassLib2
            //  - ModuleComponent -> [ AdditionalPart: ThemingModule ]

            // We want to ensure that parts are processed in the following order:
            //  - App, App.Views, ClassLib1, Component1, ModuleComponent, ThemingModule, ClassLib2

            // Here are the rules for ordering:
            // * Peers are ordered by name
            // * AdditionalParts appear immediately after the parent part.
            return EnumerateModel(entryModel).Distinct();

            IEnumerable<AssemblyPartDiscoveryModel> EnumerateModel(AssemblyPartDiscoveryModel model)
            {
                yield return model;
                var orderedAdditionalParts = model.AdditionalParts
                    .OrderByDescending(part => part, AssemblyPartDiscoveryModelComparer.Instance)
                    .ThenBy(part => part.Name, StringComparer.Ordinal);

                foreach (var additionalPart in orderedAdditionalParts)
                {
                    foreach (var item in EnumerateModel(additionalPart))
                    {
                        yield return item;
                    }
                }
            }
        }

        private class AssemblyPartDiscoveryModelComparer : IComparer<AssemblyPartDiscoveryModel>
        {
            public static AssemblyPartDiscoveryModelComparer Instance { get; } = new AssemblyPartDiscoveryModelComparer();

            public int Compare(AssemblyPartDiscoveryModel x, AssemblyPartDiscoveryModel y)
            {
                var xScore = GetScore(x);
                var yScore = GetScore(y);
                return xScore - yScore;
            }

            private static int GetScore(AssemblyPartDiscoveryModel model)
            {
                return 1 + model.AdditionalParts.Sum(part => GetScore(part));
            }
        }
    }
}
