using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using log4net;
using Rhino.ServiceBus.Internal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class DateOnlyPeriodSerializer : ICustomElementSerializer
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (DateOnlyPeriodSerializer));

        public bool CanSerialize(Type type)
        {
            return type == typeof(DateOnlyPeriod);
        }

        public XElement ToElement(object val, Func<Type, XNamespace> getNamespace)
        {
            DateOnlyPeriod period = (DateOnlyPeriod)val;
            XElement startElement = new XElement("Start", DateToString(period.StartDate.Date));
            XElement endElement = new XElement("End", DateToString(period.EndDate.Date));

            return new XElement(getNamespace(typeof(DateOnlyPeriod)) + "Period", startElement, endElement);
        }

        private static string DateToString(object val)
        {
            return ((DateTime)val).ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
        }

        public object FromElement(Type type, XElement element)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Deserializing DateOnlyPeriod object: {0}.", element.ToString(SaveOptions.None));
            }

            var start = element.Descendants("Start").First().Value;
            var end = element.Descendants("End").First().Value;

            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Deserializing DateOnlyPeriod using start: {0} and end: {1}.",start,end);
            }

            return
                new DateOnlyPeriod(
                    new DateOnly(XmlConvert.ToDateTime(start, XmlDateTimeSerializationMode.Unspecified)),
                    new DateOnly(XmlConvert.ToDateTime(end, XmlDateTimeSerializationMode.Unspecified)));
        }
    }
}