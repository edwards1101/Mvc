// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal class SourceBoundPropertyCache
    {
        private const string TempDataPrefix = "TempDataProperty-";
        private const string ViewDataPrefix = "ViewDataProperty-";

        private readonly ConcurrentDictionary<Type, IReadOnlyList<SourceBoundPropertyCacheItem>> _cache = 
            new ConcurrentDictionary<Type, IReadOnlyList<SourceBoundPropertyCacheItem>>();

        public IReadOnlyList<SourceBoundPropertyCacheItem> GetOrAdd(Type type)
        {
            if (!_cache.TryGetValue(type, out var cacheItems))
            {
                cacheItems = _cache.GetOrAdd(type, GetCacheItems(type));
            }

            return cacheItems;
        }

        private static IReadOnlyList<SourceBoundPropertyCacheItem> GetCacheItems(Type type)
        {
            var cacheItems = new List<SourceBoundPropertyCacheItem>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                if (property.GetIndexParameters().Length != 0 &&
                    property.GetMethod == null &&
                    property.SetMethod == null)
                {
                    continue;
                }

                var customAttributes = property.GetCustomAttributes(inherit: false);
                for (var j = 0; j < customAttributes.Length; j++)
                {
                    var attribute = customAttributes[j];
                    string key;
                    BoundPropertySource lifetimeKind;
                    if (attribute is ViewDataAttribute viewData)
                    {
                        key = viewData.Key ?? ViewDataPrefix + property.Name;
                        lifetimeKind = BoundPropertySource.ViewData;
                    }
                    else if (attribute is TempDataAttribute tempData)
                    {
                        key = tempData.Key ?? TempDataPrefix + property.Name;
                        lifetimeKind = BoundPropertySource.TempData;
                    }
                    else
                    {
                        continue;
                    }

                    var propertyHelper = new PropertyHelper(property);
                    cacheItems.Add(new SourceBoundPropertyCacheItem(propertyHelper, lifetimeKind, key));
                }
            }

            return cacheItems;
        }
    }
}
