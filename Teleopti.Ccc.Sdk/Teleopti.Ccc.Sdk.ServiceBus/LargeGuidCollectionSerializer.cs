using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using log4net;
using Rhino.ServiceBus.Internal;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class LargeGuidCollectionSerializer : ICustomElementSerializer
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(LargeGuidCollectionSerializer));

        public bool CanSerialize(Type type)
        {
            return typeof(ICollection<Guid>).IsAssignableFrom(type);
        }

        public XElement ToElement(object val, Func<Type, XNamespace> getNamespace)
        {
            if (val == null) throw new ArgumentNullException("val");
            if (getNamespace == null) throw new ArgumentNullException("getNamespace");

            ICollection<Guid> guidCollection = (ICollection<Guid>)val;
            if (guidCollection.Count > 25000)
                throw new ArgumentOutOfRangeException("val",
                                                      "The maximum number of items to include in the collection is 25 000.");
            var parent = new XElement(getNamespace(typeof(ICollection<Guid>)) + "GuidCollection");
            foreach (Guid guid in guidCollection)
            {
                parent.Add(new XElement("Item", guid.ToString()));
            }

            return parent;
        }

        public object FromElement(Type type, XElement element)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (element == null) throw new ArgumentNullException("element");

            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Deserializing ICollection<Guid> object: {0}.", element.ToString(SaveOptions.None));
            }

            var collection = new Collection<Guid>();
            var items = element.Descendants("Item");
            foreach (XElement xElement in items)
            {
                collection.Add(new Guid(xElement.Value));
            }

            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Returning a collection of {0} items.", collection.Count);
            }

            return collection;
        }
    }
}