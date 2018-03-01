using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    internal static class AssemblyPartDiscoveryModelFactory
    {
        public static AssemblyPartDiscoveryModel ResolveEntryPoint(Assembly entryAssembly)
        {
            var lookup = new Dictionary<Assembly, AssemblyPartDiscoveryModel>();

            var entryAssemblyModel = ResolvePartModel(entryAssembly, lookup);
            var dependencyContextProvider = new DependencyContextPartDiscoveryProvider();
            var resolvedAssemblies = dependencyContextProvider.ResolveAssemblies(entryAssembly);

            foreach (var assembly in resolvedAssemblies)
            {
                if (assembly == entryAssembly)
                {
                    // Dependency context, if present, will resolve the current executing application
                    // Ignore this value since we've already created an entry for it.
                    continue;
                }

                var partModel = ResolvePartModel(assembly, lookup);
                entryAssemblyModel.AddAdditionalPartModel(partModel);
            }

            return entryAssemblyModel;
        }

        public static AssemblyPartDiscoveryModel ResolveAssemblyModel(Assembly assembly)
        {
            var lookup = new Dictionary<Assembly, AssemblyPartDiscoveryModel>();
            return ResolvePartModel(assembly, lookup);
        }

        internal static AssemblyPartDiscoveryModel ResolvePartModel(
            Assembly root,
            Dictionary<Assembly, AssemblyPartDiscoveryModel> lookup)
        {
            var visited = new HashSet<Assembly>();
            return ResolvePartModel(root);

            AssemblyPartDiscoveryModel ResolvePartModel(Assembly assembly)
            {
                if (!visited.Add(assembly))
                {
                    throw new InvalidOperationException("Recursion");
                }

                if (lookup.TryGetValue(assembly, out var resolvedModel))
                {
                    return resolvedModel;
                }

                var model = new AssemblyPartDiscoveryModel(assembly);

                var additionalParts = model.Attributes
                    .OfType<AdditionalApplicationPartAttribute>()
                    .ToArray();

                foreach (var additionalPart in additionalParts)
                {
                    var additionalPartAssembly = Assembly.Load(new AssemblyName(additionalPart.Name));
                    var additionalPartModel = ResolvePartModel(additionalPartAssembly);

                    model.AddAdditionalPartModel(additionalPartModel);
                }

                // If the assembly has signs of using any of the new load behavior primitives, don't 
                // auto-discover precompiled views for it.
                if (additionalParts.Length == 0 || model.Attributes.OfType<ConfigureApplicationPartManagerAttribute>().Any())
                {
                    var precompiledViewAssembly = GetPrecompiledViewsAssembly(assembly);
                    if (precompiledViewAssembly != null)
                    {
                        var partModel = ResolvePartModel(precompiledViewAssembly);
                        model.AddAdditionalPartModel(partModel);
                    }
                }

                return model;
            }
        }

        private static Assembly GetPrecompiledViewsAssembly(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                return null;
            }

            for (var i = 0; i < AssemblyPartDiscoveryModel.ViewsAssemblySuffixes.Length; i++)
            {
                var fileName = assembly.GetName().Name + AssemblyPartDiscoveryModel.ViewsAssemblySuffixes[i] + ".dll";
                var filePath = Path.Combine(Path.GetDirectoryName(assembly.Location), fileName);

                if (File.Exists(filePath))
                {
                    try
                    {
                        return Assembly.LoadFile(filePath);
                    }
                    catch (FileLoadException)
                    {
                        // Don't throw if assembly cannot be loaded. This can happen if the file is not a managed assembly.
                    }
                }
            }

            return null;
        }
    }
}
