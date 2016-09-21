﻿using System;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.Payroll;

namespace Teleopti.Ccc.PayrollTest
{
    [TestFixture, Parallelizable]
    public class FormatAppenderTest
    {
        private XmlDocument document;
        private XmlDocument format;

        [SetUp]
        public void Setup()
        {
            document = new XmlDocument();
            format = new XmlDocument();
        }

        [Test]
        public void VerifyEmptyDocument()
        {
            FormatAppender.AppendFormat(document,"TeleoptiDetailedExportFormat.xml");
        }

        [Test]
        public void VerifyInvalidFormatDocument()
        {
            Assert.AreEqual(document, FormatAppender.AppendFormat(document, "TeleoptiDetailedExportFormat1.xml"));
        }

        [Test]
        public void VerifyInvalidFormatXmlDocument()
        {
            document.AppendChild(document.CreateElement("Teleopti"));
	        Assert.Throws<InvalidOperationException>(() =>
	        {
				FormatAppender.AppendFormat(document, format);
			});
        }
    }
}
