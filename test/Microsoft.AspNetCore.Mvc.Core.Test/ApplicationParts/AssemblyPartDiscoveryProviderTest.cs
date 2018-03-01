using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class AssemblyPartDiscoveryProviderTest
    {
        // Some unique assemblies
        private static readonly Assembly Assembly1 = typeof(object).Assembly;
        private static readonly Assembly Assembly2 = typeof(AssemblyPartDiscoveryProvider).Assembly;
        private static readonly Assembly Assembly3 = typeof(AssemblyPartDiscoveryProviderTest).Assembly;
        private static readonly Assembly Assembly4 = typeof(FactAttribute).Assembly;
        private static readonly Assembly Assembly5 = typeof(IActionResult).Assembly;

        [Fact]
        public void GetPartsInOrder_ReturnsEntryPart()
        {
            // Arrange
            var model = new AssemblyPartDiscoveryModel(Assembly1, "MyApp", Array.Empty<object>());

            // Act
            var parts = AssemblyPartDiscoveryProvider.GetPartsInOrder(model);

            // Assert
            Assert.Equal(new[] { model }, parts);
        }

        [Fact]
        public void GetPartsInOrder_OrdersAdditionalPartsByName()
        {
            // Arrange
            var part1 = new AssemblyPartDiscoveryModel(Assembly1, "ClassLibrary1", Array.Empty<object>());
            var part2 = new AssemblyPartDiscoveryModel(Assembly2, "ClassLibrary2", Array.Empty<object>());
            var root = new AssemblyPartDiscoveryModel(Assembly3, "MyApp", Array.Empty<object>());
            root.AddAdditionalPartModel(part2);
            root.AddAdditionalPartModel(part1);

            // Act
            var parts = AssemblyPartDiscoveryProvider.GetPartsInOrder(root);

            // Assert
            Assert.Equal(new[] { root, part1, part2 }, parts);
        }

        [Fact]
        public void GetPartsInOrder_OrdersAdditionalPartsImmediatelyAfterPart()
        {
            // Arrange
            var part1 = new AssemblyPartDiscoveryModel(Assembly1, "Library", Array.Empty<object>());
            var part2 = new AssemblyPartDiscoveryModel(Assembly2, "Library.EmbeddedFiles", Array.Empty<object>());
            var part3 = new AssemblyPartDiscoveryModel(Assembly3, "Library.Views", Array.Empty<object>());
            var root = new AssemblyPartDiscoveryModel(Assembly4, "MyApp", Array.Empty<object>());

            part1.AddAdditionalPartModel(part3);
            part1.AddAdditionalPartModel(part2);

            root.AddAdditionalPartModel(part3);
            root.AddAdditionalPartModel(part2);
            root.AddAdditionalPartModel(part1);

            // Act
            var parts = AssemblyPartDiscoveryProvider.GetPartsInOrder(root).ToArray();

            // Assert
            Assert.Equal(new[] { root, part1, part2, part3 }, parts);
        }

        [Fact]
        public void GetPartsInOrder_WorksWithDeeplyNestedParts()
        {
            // Arrange
            var part1 = new AssemblyPartDiscoveryModel(Assembly1, "Themes", Array.Empty<object>());
            var part2 = new AssemblyPartDiscoveryModel(Assembly2, "Themes.Orange", Array.Empty<object>());
            var part3 = new AssemblyPartDiscoveryModel(Assembly3, "Themes.OrangeRed", Array.Empty<object>());
            var root = new AssemblyPartDiscoveryModel(Assembly4, "MyApp", Array.Empty<object>());

            part2.AddAdditionalPartModel(part3);
            part1.AddAdditionalPartModel(part2);
            root.AddAdditionalPartModel(part1);

            // Act
            var parts = AssemblyPartDiscoveryProvider.GetPartsInOrder(root).ToArray();

            // Assert
            Assert.Equal(new[] { root, part1, part2, part3 }, parts);
        }

        [Fact]
        public void GetPartsInOrder_IgnoresAssemblyDiscoveredTwice()
        {
            // Arrange
            var part1 = new AssemblyPartDiscoveryModel(Assembly1, "ClassLibrary", Array.Empty<object>());
            var part2 = new AssemblyPartDiscoveryModel(Assembly2, "ClassLibrary.Precompiledviews", Array.Empty<object>());
            var root = new AssemblyPartDiscoveryModel(Assembly3, "MyApp", Array.Empty<object>());

            part1.AddAdditionalPartModel(part2);
            root.AddAdditionalPartModel(part1);
            root.AddAdditionalPartModel(part2);

            // Act
            var parts = AssemblyPartDiscoveryProvider.GetPartsInOrder(root).ToArray();

            // Assert
            Assert.Equal(new[] { root, part1, part2, }, parts);
        }

        [Fact]
        public void GetPartsInOrder_DiscoversAllAdditionalParts()
        {
            // Arrange
            var part1 = new AssemblyPartDiscoveryModel(Assembly1, "Library1", Array.Empty<object>());
            var part2 = new AssemblyPartDiscoveryModel(Assembly2, "Library1.EmbeddedFiles", Array.Empty<object>());
            var part3 = new AssemblyPartDiscoveryModel(Assembly3, "Library2", Array.Empty<object>());
            var part4 = new AssemblyPartDiscoveryModel(Assembly4, "Library2.Views", Array.Empty<object>());
            var root = new AssemblyPartDiscoveryModel(Assembly5, "MyApp", Array.Empty<object>());

            part1.AddAdditionalPartModel(part2);
            part3.AddAdditionalPartModel(part4);

            root.AddAdditionalPartModel(part4);
            root.AddAdditionalPartModel(part3);
            root.AddAdditionalPartModel(part1);

            // Act
            var parts = AssemblyPartDiscoveryProvider.GetPartsInOrder(root).ToArray();

            // Assert
            Assert.Equal(new[] { root, part1, part2, part3, part4 }, parts);
        }
    }
}
