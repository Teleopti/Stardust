using System;
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
            value = ApplyTypeAndFormat(itemFormat, value);
            value = ApplyQuotes(itemFormat, value);
            value = ApplyAlignAndFill(itemFormat, value);
            value = ApplyLength(itemFormat, value);
            
            return value;
        }

        public virtual string FileSuffix { get { return "txt"; } }

        private static string ApplyLength(ItemFormat format, string value)
        {
            if (format.Length>0 && value.Length>format.Length)
            {
                if (format.Align == Align.Right)
                {
                    value = value.Substring(value.Length - format.Length);
                }
                else
                {
                    value = value.Substring(0, format.Length);
                }
            }
            return value;
        }

        private static string ApplyAlignAndFill(ItemFormat format, string value)
        {
            if (format.Length > 0)
            {
                if (format.Align == Align.Right)
                    value = value.PadLeft(format.Length, format.Fill);
                else
                    value = value.PadRight(format.Length, format.Fill);
            }
            return value;
        }

        private static string ApplyQuotes(ItemFormat format, string value)
        {
            if (format.Quote)
            {
                value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{0}", "\"", value);
            }
            return value;
        }

        private static string ApplyTypeAndFormat(ItemFormat format, string value)
        {
            if (format.XmlType.Contains("date"))
            {
                if (value.Length > 0)
                {
                    DateTime localDate = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
                    if (string.IsNullOrEmpty(format.Format))
                        value = localDate.ToShortDateString();
                    else
                        value = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                              "{0:" + format.Format + "}", localDate);
                }
            }
            if (format.XmlType.Contains("time"))
            {
                if (value.Length > 0)
                {
                    TimeSpan timeSpan = XmlConvert.ToTimeSpan(value);
                    if (string.IsNullOrEmpty(format.Format))
                        value = timeSpan.ToString();
                    else
                    {
                        int hours = (int) Math.Floor(timeSpan.TotalHours);
                        value = format.Format;
                        value = value.Replace("hh",
                                              hours.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        value = value.Replace("HH",
                                              hours.ToString("00", System.Globalization.CultureInfo.InvariantCulture));
                        value = value.Replace("mm",
                                              timeSpan.Minutes.ToString("00",System.Globalization.CultureInfo.InvariantCulture));
                        value = value.Replace("ss",
                                              timeSpan.Seconds.ToString("00",System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
            }
            if (format.XmlType.Contains("decimal"))
            {
                if (value.Length > 0)
                {
                    decimal decimalValue = XmlConvert.ToDecimal(value);
                    if (string.IsNullOrEmpty(format.Format))
                        value = decimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    else
                        value = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                              "{0:" + format.Format + "}", decimalValue);
                }
            }
            if (format.XmlType.Contains("double"))
            {
                if (value.Length > 0)
                {
                    double doubleValue = XmlConvert.ToDouble(value);
                    if (string.IsNullOrEmpty(format.Format))
                        value = doubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    else
                        value = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                              "{0:" + format.Format + "}", doubleValue);
                }
            }
            return value;
        }
    }
}