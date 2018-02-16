using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public interface IPropertyLifetimeThingie
    {
        void Init(object instance, PropertyLifetimeContext context);

        void Save(object instance, PropertyLifetimeContext context);
    }

    public struct PropertyLifetimeContext
    {
        public PropertyLifetimeContext(ITempDataDictionary tempData, ViewDataDictionary viewData)
        {
            TempData = tempData;
            ViewData = viewData;
        }

        public ViewDataDictionary ViewData { get; }

        public ITempDataDictionary TempData { get; }
    }

    internal class PropertyLifetimeManager : IPropertyLifetimeThingie
    {
        private Dictionary<Type, IList<LifetimeItem>> _lookup = new Dictionary<Type, IList<LifetimeItem>>();

        public bool Initial { get; set; } = true;

        public void Init(object instance, PropertyLifetimeContext context)
        {
            var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
               .Where(property =>
               {
                   return
                       property.GetIndexParameters().Length == 0 &&
                       property.SetMethod != null &&
                       (property.IsDefined(typeof(ViewDataAttribute)) || property.IsDefined(typeof(TempDataAttribute)));
               });

            var items = new List<LifetimeItem>();
            foreach (var property in properties)
            {
                var propertyHelper = new PropertyHelper(property);
                var source = property.IsDefined(typeof(ViewDataAttribute)) ? LifetimeSource.ViewData : LifetimeSource.TempData;
                object originalValue;
                if (source == LifetimeSource.TempData)
                {
                    if (Initial)
                    {
                        originalValue = context.TempData["TempDataProperty-" + property.Name];
                    }
                    else
                    {
                        originalValue = context.TempData.Peek("TempDataProperty-" + property.Name);
                    }
                }
                else
                {
                    originalValue = context.ViewData["ViewDataProperty-" + property.Name];
                }

                if (originalValue != null)
                {
                    propertyHelper.SetValue(instance, originalValue);
                }


                items.Add(new LifetimeItem
                {
                    PropertyHelper = propertyHelper,
                    OriginalValue = originalValue,
                    Source = source,
                });
            }

            _lookup.Add(instance.GetType(), items);
            Initial = false;
        }

        public void Save(object instance, PropertyLifetimeContext context)
        {
            var items = _lookup[instance.GetType()];
            foreach (var item in items)
            {
                var currentValue = item.PropertyHelper.GetValue(instance);
                if (object.ReferenceEquals(currentValue, item.OriginalValue))
                {
                    continue;
                }

                if (item.Source == LifetimeSource.TempData)
                {
                    context.TempData["TempDataProperty-" + item.PropertyHelper.Name] = currentValue;
                }
                else
                {
                    context.ViewData["ViewDataProperty-" + item.PropertyHelper.Name] = currentValue;
                }
            }
        }

        private class LifetimeItem
        {
            public PropertyHelper PropertyHelper { get; set; }

            public object OriginalValue { get; set; }

            public LifetimeSource Source { get; set; }
        }

        private enum LifetimeSource
        {
            ViewData,
            TempData,
        }
    }
}
