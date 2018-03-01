// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationParts;

[assembly: ConfigureApplicationPartManager(typeof(NullConfigureApplicationPartManager))]
[assembly: ConfigureApplicationPartManager(typeof(MvcSandbox.LightsOnConfigureApplicationPartManager), "LightsOn")]

namespace MvcSandbox
{
    public class LightsOnConfigureApplicationPartManager : ConfigureApplicationPartManager
    {
        public override void Configure(ApplicationPartManager partManager, AssemblyPartDiscoveryModel model)
        {
            var assembly = model.Assembly;

            partManager.ApplicationParts.Add(new AssemblyPart(assembly));
            foreach (var additionalPartModel in model.AdditionalParts)
            {
                partManager.ApplicationParts.Add(additionalPartModel.ToApplicationPart());
            }
        }
    }
}
