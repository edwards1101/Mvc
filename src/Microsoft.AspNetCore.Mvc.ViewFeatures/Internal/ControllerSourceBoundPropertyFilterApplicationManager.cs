// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal class ControllerSourceBoundPropertyFilterApplicationManager : IApplicationModelProvider
    {
        /// <inheritdoc />
        /// <remarks>This order ensures that <see cref="ControllerSourceBoundPropertyFilterApplicationManager"/> runs after 
        /// the <see cref="DefaultApplicationModelProvider"/>.</remarks>
        public int Order => -1000 + 10;

        /// <inheritdoc />
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        /// <inheritdoc />
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var factory = new ServiceFilterAttribute(typeof(ControllerSourceBoundPropertyFilter))
            {
                IsReusable = true,
            };
            foreach (var controllerModel in context.Result.Controllers)
            {
                controllerModel.Filters.Add(factory);
            }
        }
    }
}
