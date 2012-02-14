using System;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture]
    public class DocumentFormatTest
    {
        private DocumentFormat target;
        private XmlDocument document;

        [SetUp]
        public void Setup()
        {
            target = new DocumentFormat("DocumentElement",DocumentFormatType.Csv);
            document = new XmlDocument();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("DocumentElement",target.Element);
            Assert.AreEqual(DocumentFormatType.Csv, target.DocumentFormatType);

            Assert.AreEqual(",",target.ItemSeparator);
            Assert.AreEqual(Environment.NewLine,target.RowSeparator);
            Assert.AreEqual(Encoding.Unicode,target.Encoding);
            Assert.IsFalse(target.HeaderEnabled);
            Assert.IsTrue(string.IsNullOrEmpty(target.HeaderElement));
            Assert.IsFalse(target.FooterEnabled);
            Assert.IsTrue(string.IsNullOrEmpty(target.FooterElement));
            Assert.AreEqual(string.Empty,target.ItemsElement);

            target.ItemSeparator = "|";
            target.RowSeparator = "#\n";
            target.SetEncoding(Encoding.ASCII.EncodingName);
            target.HeaderEnabled = true;
            target.FooterEnabled = true;
            target.HeaderElement = "header";
            target.FooterElement = "footer";
            target.ItemsElement = "record";

            Assert.AreEqual("|",target.ItemSeparator);
            Assert.AreEqual("#\n",target.RowSeparator);
            Assert.AreEqual(Encoding.ASCII,target.Encoding);
            Assert.IsTrue(target.HeaderEnabled);
            Assert.IsTrue(target.FooterEnabled);
            Assert.AreEqual("header",target.HeaderElement);
            Assert.AreEqual("footer",target.FooterElement);
            Assert.AreEqual("record",target.ItemsElement);
        }

        [Test]
        public void VerifyAddItems()
        {
            ItemFormat itemFormat = new ItemFormat("Test","xs:string");
            target.AddItem(itemFormat);

            Assert.IsTrue(target.Items.Contains(itemFormat));
        }

        [Test]
        public void VerifyLoadFromXmlWithNoFormatDefined()
        {
            document.LoadXml("<Teleopti />");
            var result = DocumentFormat.LoadFromXml(document);

            Assert.AreEqual(string.Empty,result.Element);
            Assert.AreEqual(DocumentFormatType.Xml,result.DocumentFormatType);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyLoadFromXmlWithMalformedFormat()
        {
            document.LoadXml("<Teleopti><Format /></Teleopti>");
            DocumentFormat.LoadFromXml(document);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyLoadFromXmlWithUnknownFormat()
        {
            document.LoadXml("<Teleopti><Format><Document DocumentElement='Teleopti' Type='OpenOffice' /></Format></Teleopti>");
            DocumentFormat.LoadFromXml(document);
        }

        [Test]
        public void VerifyLoadFromXmlWithCsv()
        {
            document.Load("BasicCsvExport.xml");
            DocumentFormat format = DocumentFormat.LoadFromXml(document);

            Assert.AreEqual(DocumentFormatType.Csv, format.DocumentFormatType);
            Assert.AreEqual(5,format.Items.Count);
        }

        [Test]
        public void VerifyLoadFromXmlWithXml()
        {
            document.Load("BasicXmlExport.xml");
            DocumentFormat format = DocumentFormat.LoadFromXml(document);

            Assert.AreEqual(DocumentFormatType.Xml, format.DocumentFormatType);
            Assert.AreEqual(0, format.Items.Count);
        }

        [Test]
        public void VerifyLoadFromXmlWithFixedWidth()
        {
            document.Load("BasicFixedWidthExport.xml");
            DocumentFormat format = DocumentFormat.LoadFromXml(document);

            Assert.AreEqual(DocumentFormatType.FixedWidth, format.DocumentFormatType);
            Assert.AreEqual(Encoding.GetEncoding("iso-8859-1"), format.Encoding);
            Assert.AreEqual(3, format.Items.Count);
        }
    }
}