// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class ControllerSaveTempDataPropertyFilter : SaveTempDataPropertyFilterBase, IActionFilter, IResultFilter
    {
        private readonly IPropertyLifetimeThingie _lifetimeThingie;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public ControllerSaveTempDataPropertyFilter(IModelMetadataProvider modelMetadataProvider, IPropertyLifetimeThingie lifetimeThingie, ITempDataDictionaryFactory factory)
            : base(factory)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _lifetimeThingie = lifetimeThingie;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        /// <inheritdoc />
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var tempData = _factory.GetTempData(context.HttpContext);
            var viewData = context.HttpContext.Items[ViewDataDictionaryControllerPropertyActivator.ViewDataKey] as ViewDataDictionary ??
                new ViewDataDictionary(_modelMetadataProvider, context.ModelState);

            var lifeTimeContext = new PropertyLifetimeContext(tempData, viewData);
            _lifetimeThingie.Init(context.Controller, lifeTimeContext);
            // SetPropertyVaules(tempData, Subject);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var tempData = _factory.GetTempData(context.HttpContext);
            var viewData = context.HttpContext.Items[ViewDataDictionaryControllerPropertyActivator.ViewDataKey] as ViewDataDictionary ??
                new ViewDataDictionary(_modelMetadataProvider, context.ModelState);

            var lifeTimeContext = new PropertyLifetimeContext(tempData, viewData);

            _lifetimeThingie.Save(context.Controller, lifeTimeContext);
        }
    }
}

