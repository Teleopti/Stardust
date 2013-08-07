using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Teleopti.Ccc.PayrollFormatter
{
    public abstract class PayrollFormatterBase
    {
        public abstract Stream Format(IXPathNavigable navigable, DocumentFormat documentFormat);

        protected static string FormatNodeValue(ItemFormat itemFormat, string value)
        {
            value = applyTypeAndFormat(itemFormat, value);
            value = applyQuotes(itemFormat, value);
            value = applyAlignAndFill(itemFormat, value);
            value = applyLength(itemFormat, value);
            
            return value;
        }

        public virtual string FileSuffix { get { return "txt"; } }

        private static string applyLength(ItemFormat format, string value)
        {
            if (format.Length>0 && value.Length>format.Length)
            {
	            value = format.Align == Align.Right
		                    ? value.Substring(value.Length - format.Length)
		                    : value.Substring(0, format.Length);
            }
            return value;
        }

        private static string applyAlignAndFill(ItemFormat format, string value)
        {
            if (format.Length > 0)
            {
	            value = format.Align == Align.Right
		                    ? value.PadLeft(format.Length, format.Fill)
		                    : value.PadRight(format.Length, format.Fill);
            }
            return value;
        }

        private static string applyQuotes(ItemFormat format, string value)
        {
            if (format.Quote)
            {
                value = string.Format(CultureInfo.InvariantCulture, "{0}{1}{0}", "\"", value);
            }
            return value;
        }

        private static string applyTypeAndFormat(ItemFormat format, string value)
        {
            if (format.XmlType.Contains("date"))
            {
                if (value.Length > 0)
                {
                    var localDate = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
                    if (string.IsNullOrEmpty(format.Format))
                        value = localDate.ToShortDateString();
                    else
                        value = string.Format(CultureInfo.InvariantCulture,
                                              "{0:" + format.Format + "}", localDate);
                }
            }
			if (format.XmlType.Contains("fullTime"))
			{
				if (value.Length > 0)
				{
					var localdateTime = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
					if (string.IsNullOrEmpty(format.Format))
						value = localdateTime.ToString("u");
					else
						value = string.Format(CultureInfo.InvariantCulture,
						                      "{0:" + format.Format + "}", localdateTime);
				}
			}
            if (format.XmlType.Contains("time"))
            {
                if (value.Length > 0)
                {
                    var timeSpan = XmlConvert.ToTimeSpan(value);
                    if (string.IsNullOrEmpty(format.Format))
                        value = timeSpan.ToString();
                    else
                    {
                        var hours = (int) Math.Floor(timeSpan.TotalHours);
                        value = format.Format;
                        value = value.Replace("hh",
                                              hours.ToString(CultureInfo.InvariantCulture));
                        value = value.Replace("HH",
                                              hours.ToString("00", CultureInfo.InvariantCulture));
                        value = value.Replace("mm",
                                              timeSpan.Minutes.ToString("00",CultureInfo.InvariantCulture));
                        value = value.Replace("ss",
                                              timeSpan.Seconds.ToString("00",CultureInfo.InvariantCulture));
                    }
                }
            }
            if (format.XmlType.Contains("decimal"))
            {
                if (value.Length > 0)
                {
                    var decimalValue = XmlConvert.ToDecimal(value);
                    if (string.IsNullOrEmpty(format.Format))
                        value = decimalValue.ToString(CultureInfo.InvariantCulture);
                    else
                        value = string.Format(CultureInfo.InvariantCulture,
                                              "{0:" + format.Format + "}", decimalValue);
                }
            }
            if (format.XmlType.Contains("double"))
            {
                if (value.Length > 0)
                {
                    var doubleValue = XmlConvert.ToDouble(value);
                    if (string.IsNullOrEmpty(format.Format))
                        value = doubleValue.ToString(CultureInfo.InvariantCulture);
                    else
                        value = string.Format(CultureInfo.InvariantCulture,
                                              "{0:" + format.Format + "}", doubleValue);
                }
            }
            return value;
        }
    }
}