﻿using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture, Parallelizable]
    public class ExcelFormatterTest
    {
        private ExcelFormatter target;

        [SetUp]
        public void Setup()
        {
            target = new ExcelFormatter(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
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
            document.Load(Path.Combine(TestContext.CurrentContext.TestDirectory, "BasicExcelExport.xml"));

            DocumentFormat format = DocumentFormat.LoadFromXml(document);
            Stream stream = target.Format(document, format);
            stream.Position = 0;

            StreamReader streamReader = new StreamReader(stream, format.Encoding);
            string content = streamReader.ReadToEnd();

            Assert.AreEqual(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "BasicExcelExportResult.txt")).Replace("\r\n","\n"), content);
            Assert.AreEqual("xls", target.FileSuffix);
        }
    }
}
