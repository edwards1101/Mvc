// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal struct SourceBoundPropertyCacheItem
    {
        public SourceBoundPropertyCacheItem(
            PropertyHelper propertyHelper, 
            BoundPropertySource source,
            string sourceKey)
        {
            PropertyHelper = propertyHelper;
            Source = source;
            SourceKey = sourceKey;
        }

        public PropertyHelper PropertyHelper { get; }

        public BoundPropertySource Source { get; }

        public string SourceKey { get; set; }
    }
}
