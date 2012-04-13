using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Rhino.ServiceBus.Internal;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class LargeForecastsRowCollectionSerializer : ICustomElementSerializer
    {
        public bool CanSerialize(Type type)
        {
            return typeof(ICollection<IForecastsRow>).IsAssignableFrom(type);
        }

        public XElement ToElement(object val, Func<Type, XNamespace> getNamespace)
        {
            if (val == null) throw new ArgumentNullException("val");
            if (getNamespace == null) throw new ArgumentNullException("getNamespace");

            var forecastsCollection = (ICollection<IForecastsRow>)val;
            if (forecastsCollection.Count > 300)
                throw new ArgumentOutOfRangeException("val", "The maximum number of items to include in the collection is 300.");
            var parent = new XElement(getNamespace(typeof(ICollection<IForecastsRow>)) + "ForecastsRowCollection");
            foreach (var forcasts in forecastsCollection)
            {
                parent.Add(new XElement("Item", forcasts.ToString()));
            }

            return parent;
        }

        public object FromElement(Type type, XElement element)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (element == null) throw new ArgumentNullException("element");

            var collection = new Collection<IForecastsRow>();
            var items = element.Descendants("Item");
            foreach (XElement xElement in items)
            {
                collection.Add(new ForecastsRow(xElement.Value));
            }

            return collection;
        }
    }
}
