// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    internal class PageSourceBoundPropertyApplicationModelProvider : IPageApplicationModelProvider
    {
        private readonly ServiceFilterAttribute _filter;

        /// <summary>
        /// Ordered to execute after <see cref="DefaultPageApplicationModelProvider"/>.
        /// </summary>
        public int Order => -1000 + 10;

        public PageSourceBoundPropertyApplicationModelProvider()
        {
            _filter = new ServiceFilterAttribute(typeof(PageSourceBoundPropertyManagerFilter))
            {
                IsReusable = true,
            };
        }

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
            
        }

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {

            context.PageApplicationModel.Filters.Add(_filter);
        }
    }
}
