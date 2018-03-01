using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Hosting;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    internal class CompiledViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            var knownIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in parts.OfType<CompiledViewsApplicationPart>())
            {
                var assembly = part.Assembly;
                var attributes = assembly.GetCustomAttributes<RazorViewAttribute>();
                var items = LoadItems(assembly);

                var merged = Merge(items, attributes);
                foreach (var item in merged)
                {
                    var descriptor = new CompiledViewDescriptor(item.item, item.attribute);
                    // We iterate through ApplicationPart instances appear in precendence order.
                    // If a view path appears in multiple views, we'll use the order to break ties.
                    if (knownIdentifiers.Add(descriptor.RelativePath))
                    {
                        feature.ViewDescriptors.Add(descriptor);
                    }
                }
            }
        }

        private ICollection<(RazorCompiledItem item, RazorViewAttribute attribute)> Merge(
            IReadOnlyList<RazorCompiledItem> items,
            IEnumerable<RazorViewAttribute> attributes)
        {
            // This code is a intentionally defensive. We assume that it's possible to have duplicates
            // of attributes, and also items that have a single kind of metadata, but not the other.
            var dictionary = new Dictionary<string, (RazorCompiledItem item, RazorViewAttribute attribute)>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!dictionary.TryGetValue(item.Identifier, out var entry))
                {
                    dictionary.Add(item.Identifier, (item, null));

                }
                else if (entry.item == null)
                {
                    dictionary[item.Identifier] = (item, entry.attribute);
                }
            }

            foreach (var attribute in attributes)
            {
                if (!dictionary.TryGetValue(attribute.Path, out var entry))
                {
                    dictionary.Add(attribute.Path, (null, attribute));
                }
                else if (entry.attribute == null)
                {
                    dictionary[attribute.Path] = (entry.item, attribute);
                }
            }

            return dictionary.Values;
        }

        internal IReadOnlyList<RazorCompiledItem> LoadItems(Assembly assembly)
        {
            var loader = new RazorCompiledItemLoader();
            return loader.LoadItems(assembly);

        }
    }
}
