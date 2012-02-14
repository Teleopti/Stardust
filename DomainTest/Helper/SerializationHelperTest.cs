using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class SerializationHelperTest
    {
        //[Ignore("Henry, Roger. Varför skulle inte detta funka nu då")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test] 
        public void VerifySerializeXml()
        {
            SerializationTestClass person = new SerializationTestClass();
            person.Note = "A note";
            string xml = SerializationHelper.SerializeAsXml(person);
            Assert.IsNotEmpty(xml);
        }

        //[Ignore("Henry, Roger. Varför skulle inte detta funka nu då")]
        [Test] 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifyDeserializeXml()
        {
            const string xml = "﻿<?xml version=\"1.0\" encoding=\"utf-8\"?><SerializationTestClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TerminalDate xsi:nil=\"true\" /><Note>A note</Note></SerializationTestClass>";
            SerializationTestClass person = SerializationHelper.Deserialize<SerializationTestClass>(xml);
            Assert.IsNotNull(person);
            Assert.AreEqual("A note",person.Note);
            Assert.IsFalse(person.TerminalDate.HasValue);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifySerializeAndDeserializeBinary()
        {
            ICccTimeZoneInfo cccTimeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.Utc);
            byte[] serializedValue = SerializationHelper.SerializeAsBinary(cccTimeZoneInfo);
            Assert.Greater(serializedValue.Length,0);
            cccTimeZoneInfo = SerializationHelper.Deserialize<ICccTimeZoneInfo>(serializedValue);
            Assert.AreEqual(TimeZoneInfo.Utc.Id, cccTimeZoneInfo.Utc.Id);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This should only be used for this test!")]
        public class SerializationTestClass
        {
            public string Note { get; set;}
            public DateTime? TerminalDate { get; set; }
        }
    }
}
