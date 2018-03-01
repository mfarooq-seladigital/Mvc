using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class AssemblyPartDiscoveryModel : IEquatable<AssemblyPartDiscoveryModel>
    {
        internal static readonly string[] ViewsAssemblySuffixes = new[]
        {
            ".PrecompiledViews",
            ".Views",
        };

        private readonly List<AssemblyPartDiscoveryModel> _additionalParts;

        public AssemblyPartDiscoveryModel(Assembly assembly)
            : this(assembly, assembly.GetName().Name, assembly.GetCustomAttributes(inherit: false))
        {
        }

        public AssemblyPartDiscoveryModel(
            Assembly assembly,
            string name,
            IReadOnlyList<object> attributes)
        {
            Assembly = assembly;
            Name = name;
            Attributes = attributes;
            _additionalParts = new List<AssemblyPartDiscoveryModel>();
        }

        public string Name { get; }

        public Assembly Assembly { get; }

        public IReadOnlyList<object> Attributes { get; }

        public IReadOnlyList<AssemblyPartDiscoveryModel> AdditionalParts => _additionalParts;

        internal void AddAdditionalPartModel(AssemblyPartDiscoveryModel additionalPart)
        {
            _additionalParts.Add(additionalPart);
        }

        public ApplicationPart ToApplicationPart()
        {
            for (var i = 0; i < ViewsAssemblySuffixes.Length; i++)
            {
                if (Name.EndsWith(ViewsAssemblySuffixes[i], StringComparison.OrdinalIgnoreCase))
                {
                    return new CompiledViewsApplicationPart(Assembly);
                }
            }

            return new AssemblyPart(Assembly);
        }

        public override string ToString() => Name;

        public override bool Equals(object obj)
        {
            return (obj is AssemblyPartDiscoveryModel model) && Equals(model);
        }

        public bool Equals(AssemblyPartDiscoveryModel other)
        {
            return Assembly.Equals(other?.Assembly);
        }

        public override int GetHashCode()
        {
            return Assembly.GetHashCode();
        }
    }
}
