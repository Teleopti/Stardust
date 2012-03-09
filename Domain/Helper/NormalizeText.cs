using System.Xml;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
    public class NormalizeText : ITextFormatter
    {
        /// <summary>
        /// Strange Normalize method, this is the only way I found to fully Normalize the string,
        /// to work with WebServices when sent to MyTime.... 
        /// The XMLDocument class makes the string normalized /Peter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string Format(string value)
        {
            var doc = new XmlDocument();
            var message = doc.CreateElement("A");
            message.InnerText = value;
            return message.InnerXml;
        }
    }
}
