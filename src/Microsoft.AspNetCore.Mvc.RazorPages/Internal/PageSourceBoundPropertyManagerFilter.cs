// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    internal class PageSourceBoundPropertyManagerFilter : IPageFilter, IResultFilter
    {
        private static readonly object ViewDataDictionaryKey = typeof(ViewDataDictionary);
        private readonly ISourceBoundPropertyManager _manager;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public PageSourceBoundPropertyManagerFilter(
            ISourceBoundPropertyManager manager,
            ITempDataDictionaryFactory tempDataFactory)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _tempDataFactory = tempDataFactory ?? throw new ArgumentNullException(nameof(tempDataFactory));
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            // Stash ViewData so we can read it during result execution.
            context.HttpContext.Items[ViewDataDictionaryKey] = context.ViewData;

            var propertyContext = new SourceBoundPropertyContext(context, tempData, context.ViewData);
            _manager.Populate(context.HandlerInstance, propertyContext);
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            var viewData = (ViewDataDictionary)context.HttpContext.Items[ViewDataDictionaryKey];

            var propertyContext = new SourceBoundPropertyContext(context, tempData, viewData);
            _manager.Save(context.Controller, propertyContext);
        }
    }
}
