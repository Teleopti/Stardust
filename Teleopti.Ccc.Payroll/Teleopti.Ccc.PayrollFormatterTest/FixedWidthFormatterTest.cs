﻿using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture, Parallelizable]
    public class FixedWidthFormatterTest
    {
        private FlatFileFormatter target;

        [SetUp]
        public void Setup()
        {
            target = new FlatFileFormatter(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }

        [Test]
        public void VerifyBasicFixedWidthExport()
        {
            XmlDocument document = new XmlDocument();
            document.Load(Path.Combine(TestContext.CurrentContext.TestDirectory, "BasicFixedWidthExport.xml"));

            DocumentFormat format = DocumentFormat.LoadFromXml(document);
            Stream result = target.Format(document, format);
            result.Position = 0;

            StreamReader streamReader = new StreamReader(result, format.Encoding);
            string content = streamReader.ReadToEnd();

            Assert.AreEqual(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "BasicFixedWidthExportResult.txt")).Replace("\r",""),content);
            Assert.AreEqual("txt", target.FileSuffix);
        }
    }
}
