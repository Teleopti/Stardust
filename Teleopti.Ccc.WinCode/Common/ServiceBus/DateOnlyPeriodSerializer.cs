using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Rhino.ServiceBus.Internal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.ServiceBus
{
	public class DateOnlyPeriodSerializer : ICustomElementSerializer
	{
		public bool CanSerialize(Type type)
		{
			return type == typeof(DateOnlyPeriod);
		}

		public XElement ToElement(object val, Func<Type, XNamespace> getNamespace)
		{
			if (val == null) throw new ArgumentNullException("val");
			if (getNamespace == null) throw new ArgumentNullException("getNamespace");

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
			if (type == null) throw new ArgumentNullException("type");
			if (element == null) throw new ArgumentNullException("element");

			var start = element.Descendants("Start").First().Value;
			var end = element.Descendants("End").First().Value;

			return
				new DateOnlyPeriod(
					new DateOnly(XmlConvert.ToDateTime(start, XmlDateTimeSerializationMode.Unspecified)),
					new DateOnly(XmlConvert.ToDateTime(end, XmlDateTimeSerializationMode.Unspecified)));
		}
	}
}