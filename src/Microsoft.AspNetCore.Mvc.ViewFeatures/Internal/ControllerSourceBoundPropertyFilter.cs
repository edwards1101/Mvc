// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal class ControllerSourceBoundPropertyFilter : IActionFilter, IResultFilter
    {
        private readonly ISourceBoundPropertyManager _manager;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly ControllerViewDataDictionaryFactory _viewDataFactory;

        public ControllerSourceBoundPropertyFilter(
            ISourceBoundPropertyManager manager, 
            ITempDataDictionaryFactory tempDataFactory, 
            ControllerViewDataDictionaryFactory viewDataFactory)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _tempDataFactory = tempDataFactory ?? throw new ArgumentNullException(nameof(tempDataFactory));
            _viewDataFactory = viewDataFactory ?? throw new ArgumentNullException(nameof(viewDataFactory));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            var viewData = _viewDataFactory.GetViewDataDictionary(context);

            var propertySourceContext = new SourceBoundPropertyContext(context, tempData, viewData);
            _manager.Populate(context.Controller, propertySourceContext);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            var viewData = _viewDataFactory.GetViewDataDictionary(context);

            var propertySourceContext = new SourceBoundPropertyContext(context, tempData, viewData);
            _manager.Save(context.Controller, propertySourceContext);
        }
    }
}

