// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ConfigureApplicationPartManagerAttribute : Attribute
    {
        public static readonly string DefaultName = "Default";

        public ConfigureApplicationPartManagerAttribute(Type type)
            : this(type, name: DefaultName)
        {
        }

        public ConfigureApplicationPartManagerAttribute(Type type, string name)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Type Type { get; }

        public string Name { get; }
    }
}
