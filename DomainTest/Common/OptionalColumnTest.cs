using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class OptionalColumnTest
    {
        private OptionalColumn _optionalColumn;
    	private string _columnName;

    	[SetUp]
        public void Setup()
    	{
    		_columnName = "OptionalColumn";
			_optionalColumn = new OptionalColumn(_columnName);
        }

        [Test]
        public void VerifyCanCreate()
        {
            Assert.AreEqual(_columnName, _optionalColumn.Name);
        }

        [Test]
        public void VerifyNameCanSet()
        {
            _optionalColumn = new OptionalColumn(_columnName) {Name = _columnName};
        	Assert.AreEqual(_columnName, _optionalColumn.Name );
        }

        [Test]
        public void VerifyTableNameCanSet()
        {
            const string tableName = "Person";
        	_optionalColumn = new OptionalColumn(_columnName) {TableName = tableName};
        	Assert.AreEqual(tableName, _optionalColumn.TableName);
        }

		[Test]
		public void VerifyAvailableAsGroupPageCanSet()
		{
			_optionalColumn = new OptionalColumn(_columnName)
			{
				Name = _columnName,
				AvailableAsGroupPage = true
			};
			Assert.AreEqual(true, _optionalColumn.AvailableAsGroupPage);
		}

		[Test]
		public void ShouldPublishTwoEventsForColumnValueChangesOnTwoPersons()
		{
			_optionalColumn = new OptionalColumn(_columnName)
			{
				Name = _columnName
			};
			var person1 = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var person2 = PersonFactory.CreatePersonWithId(Guid.NewGuid());

			_optionalColumn.TriggerValueChangeForPerson(person1);
			_optionalColumn.TriggerValueChangeForPerson(person2);

			var events = _optionalColumn.PopAllEvents().OfType<OptionalColumnValueChangedEvent>().ToList();

			events.Count.Should().Be(2);
			events.First().PersonId.Should().Be(person1.Id.GetValueOrDefault());
			events.Last().PersonId.Should().Be(person2.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldOnlyPublishOneEventForSeveralColumnValueChangesOnOnePerson()
		{
			_optionalColumn = new OptionalColumn(_columnName)
			{
				Name = _columnName
			};
			var person1 = PersonFactory.CreatePersonWithId(Guid.NewGuid());

			_optionalColumn.TriggerValueChangeForPerson(person1);
			_optionalColumn.TriggerValueChangeForPerson(person1);

			_optionalColumn.PopAllEvents().OfType<OptionalColumnValueChangedEvent>().Count()
				.Should().Be(1);
		}
	}
}
