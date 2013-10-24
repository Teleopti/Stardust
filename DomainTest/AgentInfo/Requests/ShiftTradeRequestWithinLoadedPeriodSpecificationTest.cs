using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[TestFixture]
	public class ShiftTradeRequestWithinLoadedPeriodSpecificationTest
	{
		private ShiftTradeRequestIsAfterLoadedPeriodSpecification _target;
		private IPerson _person1, _person2;

		[SetUp]
		public void Setup()
		{
			_person1 = PersonFactory.CreatePerson();
			_person2 = PersonFactory.CreatePerson();
			var dateTimePeriod = new DateTimePeriod(2013, 10, 1, 2013, 10, 31);
			_target = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(dateTimePeriod);
		}

		[Test]
		public void IsSatisfiedBy_RequestWithinPeriod_ReturnFalse()
		{
			var dateOnly = new DateOnly(2013, 10, 15);
			var personRequest = createShiftTrade(dateOnly, dateOnly);

			var result = _target.IsSatisfiedBy(personRequest);
			result.Should().Be.False();
		}

		[Test]
		public void IsSatisfiedBy_RequestOutsidePeriod_ReturnTrue()
		{
			var dateOnly = new DateOnly(2013, 09, 01);
			var personRequest = createShiftTrade(dateOnly, dateOnly);

			var result = _target.IsSatisfiedBy(personRequest);
			result.Should().Be.True();
		}

		[Test]
		public void IsSatisfiedBy_RequestParitiallyWithinPeriod_ReturnFalse()
		{
			var startDate = new DateOnly(2013, 09, 30);
			var endDate = new DateOnly(2013, 10, 01);
			var personRequest = createShiftTrade(startDate, endDate);

			var result = _target.IsSatisfiedBy(personRequest);
			result.Should().Be.False();
		}

		[Test]
		public void IsSatisfiedBy_RequestIsNull_ReturnTrue()
		{
			var result = _target.IsSatisfiedBy(null);
			result.Should().Be.True();
		}

		[Test]
		public void IsSatisFiedBy_RequestIsNotShiftTrade_ReturnTrue()
		{
			var request = new PersonRequest(_person1, new TextRequest(new DateTimePeriod(2013, 10, 15, 2013, 10, 16)));

			var result = _target.IsSatisfiedBy(request);
			result.Should().Be.True();
		}

		private PersonRequest createShiftTrade(DateOnly startDate, DateOnly endDate)
		{
			return new PersonRequest(_person1, new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(_person1, _person2, startDate, endDate)
				}));
		}
	}
}
