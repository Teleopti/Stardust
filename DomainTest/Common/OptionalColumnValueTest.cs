using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class OptionalColumnValueTest
    {
        private OptionalColumnValue _value;

        [Test]
        public void Setup()
        {
            _value = new OptionalColumnValue("Value");
        }

        [Test]
        public void VerifyCanCreate()
        {
            const string columnValue = "Value";
            _value = new OptionalColumnValue(columnValue);

            Assert.AreEqual(columnValue, _value.Description);
        }

        [Test]
        public void VerifyDescriptionCanSet()
        {
            const string columnValue = "Value";
        	_value = new OptionalColumnValue(columnValue) {Description = columnValue};
        	Assert.AreEqual(columnValue, _value.Description);
        }

        [Test]
        public void VerifyReferenceIdCanSet()
        {
            const string columnValue = "Value";
        	var person = new Person();

        	_value = new OptionalColumnValue(columnValue) {ReferenceObject = person};

        	Assert.That(_value.ReferenceObject, Is.EqualTo(person));
        }
    }
}
