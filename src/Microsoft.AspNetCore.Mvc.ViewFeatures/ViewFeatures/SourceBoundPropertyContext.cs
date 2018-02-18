// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public struct SourceBoundPropertyContext
    {
        public SourceBoundPropertyContext(ActionContext actionContext, ITempDataDictionary tempData, ViewDataDictionary viewData)
        {
            ActionContext = actionContext ?? throw new ArgumentNullException(nameof(actionContext));
            TempData = tempData ?? throw new ArgumentNullException(nameof(tempData));
            ViewData = viewData ?? throw new ArgumentNullException(nameof(viewData));
        }

        public ActionContext ActionContext { get; }

        public ViewDataDictionary ViewData { get; }

        public ITempDataDictionary TempData { get; }
    }
}
