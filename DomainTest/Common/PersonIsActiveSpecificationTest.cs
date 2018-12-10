using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class PersonIsActiveSpecificationTest
	{
		private PersonIsActiveSpecification _target;
		private DateOnly _dayBeforeTerminalDate;
		private DateOnly _dayAfterTerminalDate;
		private IPerson _person;
		private DateOnly _terminalDate;

		[SetUp]
		public void Setup()
		{
			_person = PersonFactory.CreatePerson();
			_terminalDate = new DateOnly(2001, 02, 01);
			_dayBeforeTerminalDate = new DateOnly(2001, 01, 01);
			_dayAfterTerminalDate = new DateOnly(2001, 03, 01);
		}

		[Test]
		public void ShouldBeSatisfiedWithNoTerminalDate()
		{
			Assert.IsNull(_person.TerminalDate);
			_target = new PersonIsActiveSpecification(_dayBeforeTerminalDate);
			Assert.IsTrue(_target.IsSatisfiedBy(_person));
		}


		[Test]
		public void ShouldNotBeSatisfiedWithTerminalDateAfterDayInQuestion()
		{
			_person.TerminatePerson(_terminalDate, new PersonAccountUpdaterDummy());
			_target = new PersonIsActiveSpecification(_dayAfterTerminalDate);
			Assert.IsFalse(_target.IsSatisfiedBy(_person));
		}

		[Test]
		public void ShouldBeSatisfiedWithTerminalDateBeforeDayInQuestion()
		{
			_person.TerminatePerson(_terminalDate, new PersonAccountUpdaterDummy());
			_target = new PersonIsActiveSpecification(_dayBeforeTerminalDate);
			Assert.IsTrue(_target.IsSatisfiedBy(_person));
		}

	}
}
