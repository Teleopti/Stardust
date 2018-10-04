﻿using System.IO;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture, Parallelizable]
    public class XmlFormatterTest
    {
        private XmlFormatter target;

        [SetUp]
        public void Setup()
        {
            target = new XmlFormatter();
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyThroughputResult()
        {
            XmlDocument document = new XmlDocument();
			var testFilesDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
			document.Load(Path.Combine(testFilesDir, "BasicXmlExport.xml"));

            DocumentFormat format = DocumentFormat.LoadFromXml(document);
            Stream stream = target.Format(document, format);
            StreamReader streamReader = new StreamReader(stream, format.Encoding);
            string content = streamReader.ReadToEnd();

            Assert.AreEqual(File.ReadAllText(Path.Combine(testFilesDir, "BasicXmlExport.xml")),content);
            Assert.AreEqual("xml", target.FileSuffix);
        }
    }
}
