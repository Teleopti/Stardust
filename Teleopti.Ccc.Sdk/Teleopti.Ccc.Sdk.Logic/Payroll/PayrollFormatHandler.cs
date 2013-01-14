using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Payroll
{
    public class PayrollFormatHandler
    {
        private readonly string _path;
        private const string _payrollPath = "Teleopti.Payroll";
        private const string _fileName = "internal.storage.xml";
        private readonly string _filePath;

        public PayrollFormatHandler(string path)
        {
            _path = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", path, _payrollPath);
            _filePath = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", _path, _fileName);
        }

        public void Save(ICollection<PayrollFormatDto> payrollFormatDtos)
        {
            if (payrollFormatDtos == null) return;
            
            var xmlDoc = new XmlDocument();

            var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            var rootNode = xmlDoc.CreateElement("Payroll");
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
            xmlDoc.AppendChild(rootNode);

            foreach (var payrollFormatDto in payrollFormatDtos)
            {
                var node = xmlDoc.CreateElement("PayrollFormat");
                var attrFormatId = xmlDoc.CreateAttribute("Id");
                attrFormatId.Value = string.Format(CultureInfo.InvariantCulture, "{0}", payrollFormatDto.FormatId);

                var attrName = xmlDoc.CreateAttribute("Name");
                attrName.Value = payrollFormatDto.Name;

				var attrDataSource = xmlDoc.CreateAttribute("DataSource");
				attrDataSource.Value = payrollFormatDto.DataSource;

                node.Attributes.Append(attrFormatId);
                node.Attributes.Append(attrName);
				node.Attributes.Append(attrDataSource);
                rootNode.AppendChild(node);
            }
            xmlDoc.Save(_filePath);
        }

        public ICollection<PayrollFormatDto> Load(string dataSource)
        {
            if (File.Exists(_filePath))
            {
                var list =  (from format in XElement.Load(_filePath).Elements("PayrollFormat")
                                                            select
                                                                new PayrollFormatDto(new Guid(format.Attribute("Id").Value),
                                                                                     format.Attribute("Name").Value, format.Attribute("DataSource").Value)).ToList();
            	return list.Where(payrollFormatDto => (string.IsNullOrEmpty(payrollFormatDto.DataSource) || payrollFormatDto.DataSource.Equals(dataSource))).ToList();
            }

            return new List<PayrollFormatDto>();
        }
    }
}