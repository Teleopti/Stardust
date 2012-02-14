using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.DomainTest.Helper;

using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class OptionalColumnValueTest
    {
        private OptionalColumnValue value;

        [Test]
        public void Setup()
        {
            value = new OptionalColumnValue("Value");
        }

        [Test]
        public void VerifyCanCreate()
        {
            string columnValue = "Value";
            value = new OptionalColumnValue(columnValue);

            Assert.AreEqual(columnValue, value.Description);
        }

        [Test]
        public void VerifyDescriptionCanSet()
        {
            string columnValue = "Value";
            value = new OptionalColumnValue(columnValue);
            value.Description = columnValue;
            Assert.AreEqual(columnValue, value.Description);
        }

        [Test]
        public void VerifyReferenceIdCanSet()
        {
            string columnValue = "Value";
            Guid? id = Guid.NewGuid();

            value = new OptionalColumnValue(columnValue);
            value.ReferenceId = id;

            Assert.AreEqual(id, value.ReferenceId);
        }
    }
}
