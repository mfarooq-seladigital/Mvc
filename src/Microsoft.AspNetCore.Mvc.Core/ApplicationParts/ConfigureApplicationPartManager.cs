// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public abstract class ConfigureApplicationPartManager
    {
        public abstract void Configure(ApplicationPartManager partManager, AssemblyPartDiscoveryModel model);

        public static ConfigureApplicationPartManager GetConfigureOperation(AssemblyPartDiscoveryModel partDiscoveryModel)
            => GetConfigureOperation(partDiscoveryModel, ConfigureApplicationPartManagerAttribute.DefaultName);

        public static ConfigureApplicationPartManager GetConfigureOperation(AssemblyPartDiscoveryModel partModel, string name)
        { 
            var configureAttribute = partModel.Attributes.OfType<ConfigureApplicationPartManagerAttribute>()
                .FirstOrDefault(attribute => string.Equals(attribute.Name, name, StringComparison.Ordinal));

            if (configureAttribute?.Type != null)
            {
                var type = configureAttribute.Type;
                if (!typeof(ConfigureApplicationPartManager).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException($"Type {type} specified in {nameof(ConfigureApplicationPartManagerAttribute)}[name={name} " +
                        "is not a subclass of {nameof(ConfigureApplicationPartManager)}");
                }

                return (ConfigureApplicationPartManager)Activator.CreateInstance(configureAttribute.Type);
            }
            else
            {
                return new DefaultConfigureApplicationPartManager();
            }
        }

        private class DefaultConfigureApplicationPartManager : ConfigureApplicationPartManager
        {
            public override void Configure(ApplicationPartManager partManager, AssemblyPartDiscoveryModel model)
            {
                var part = model.ToApplicationPart();
                partManager.ApplicationParts.Add(part);
            }
        }
    }
}
