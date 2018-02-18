// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal class SourceBoundPropertyManager : ISourceBoundPropertyManager
    {
        private readonly SourceBoundPropertyCache _cache;

        public SourceBoundPropertyManager(SourceBoundPropertyCache cache)
        {
            _cache = cache;
        }

        public void Populate(object instance, SourceBoundPropertyContext context)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var cacheItems = _cache.GetOrAdd(instance.GetType());
            for (var i = 0; i < cacheItems.Count; i++)
            {
                var cacheItem = cacheItems[i];
                var propertyName = cacheItem.PropertyHelper.Name;

                object value;
                switch (cacheItem.Source)
                {
                    case BoundPropertySource.TempData:
                        value = context.TempData[cacheItem.SourceKey];
                        break;
                    case BoundPropertySource.ViewData:
                        value = context.ViewData[cacheItem.SourceKey];
                        break;
                    default:
                        throw new InvalidOperationException(Resources.FormatUnsupportedEnumType(cacheItem.Source));
                }

                if (value == null)
                {
                    continue;
                }

                cacheItem.PropertyHelper.SetValue(instance, value);
            }
        }

        public void Save(object instance, SourceBoundPropertyContext context)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var cacheItems = _cache.GetOrAdd(instance.GetType());
            for (var i = 0; i < cacheItems.Count; i++)
            {
                var cacheItem = cacheItems[i];
                var key = cacheItem.SourceKey;

                var currentValue = cacheItem.PropertyHelper.GetValue(instance);
                if (cacheItem.Source == BoundPropertySource.TempData)
                {
                    var originalValue = context.TempData[key];
                    if (!object.ReferenceEquals(originalValue, currentValue))
                    {
                        context.TempData[key] = currentValue;
                        // Mark the key to be kept. This ensures that even if something later in the execution pipeline reads it,
                        // such as another view with a `TempData` property, the key is preserved through the current request.
                        context.TempData.Keep(key);
                    }
                }
                else if (cacheItem.Source == BoundPropertySource.ViewData)
                {
                    var originalValue = context.ViewData[key];
                    if (!object.ReferenceEquals(originalValue, currentValue))
                    {
                        context.ViewData[key] = currentValue;
                    }
                }
            }
        }
    }
}
