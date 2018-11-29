using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[TestFixture]
	public class ShiftTradeRequestIsAfterLoadedPeriodSpecificationTest
	{
		private ShiftTradeRequestIsAfterLoadedPeriodSpecification _target;
		
		[Test]
		public void IsSatisfiedBy_RequestIsNull_ReturnTrue()
		{
			var loadedPeriod = new DateTimePeriod(2013, 10, 1, 2013, 10, 31);
			_target = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(loadedPeriod);
			_target.IsSatisfiedBy(null).Should().Be(false);
		}

		[Test]
		public void IsSatisFiedBy_RequestIsNotShiftTrade_ReturnTrue()
		{
			var loadedPeriod = new DateTimePeriod(2013, 10, 1, 2013, 10, 31);
			_target = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(loadedPeriod);
			var person = PersonFactory.CreatePerson();
			var request = new PersonRequest(person, new TextRequest(new DateTimePeriod(2013, 10, 15, 2013, 10, 16)));

			_target.IsSatisfiedBy(request).Should().Be(false);
		}

		[Test]
		public void IsSatisfiedBy_RequestWithinPeriod_ReturnFalse()
		{
			var loadedPeriod = new DateTimePeriod(2013, 10, 1, 2013, 10, 31);
			_target = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(loadedPeriod);
			var request = createShiftTrade(new DateOnly(2013, 10, 15), new DateOnly(2013, 10, 16));

			_target.IsSatisfiedBy(request).Should().Be(false);
		}

		[Test]
		public void IsSatisfiedBy_RequestAfterPeriodAndReferred_ReturnFalse()
		{
			var loadedPeriod = new DateTimePeriod(2013, 10, 01, 2013, 10, 31);
			_target = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(loadedPeriod);
			var request = createShiftTrade(new DateOnly(2013, 11, 10), new DateOnly(2013, 11, 11));
			request.ForcePending();
			((IShiftTradeRequest)request.Request).SetShiftTradeStatus(ShiftTradeStatus.Referred, new PersonRequestAuthorizationCheckerForTest());

			_target.IsSatisfiedBy(request).Should().Be(false);
		}

		[Test]
		public void IsSatisfiedBy_RequestAfterPeriodAndOkByMe_ReturnFalse()
		{
			var loadedPeriod = new DateTimePeriod(2013, 10, 01, 2013, 10, 31);
			_target = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(loadedPeriod);
			var request = createShiftTrade(new DateOnly(2013, 11, 10), new DateOnly(2013, 11, 11));
			request.ForcePending();
			((IShiftTradeRequest)request.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByMe, new PersonRequestAuthorizationCheckerForTest());

			_target.IsSatisfiedBy(request).Should().Be(false);
		}

		[Test]
		public void IsSatisfiedBy_RequestPartiallyInsideAndOkByMe_ReturnFalse()
		{
			var loadedPeriod = new DateTimePeriod(2013, 10, 01, 2013, 10, 31);
			_target = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(loadedPeriod);
			var person = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var request = new PersonRequest(person, new ShiftTradeRequest(
				                                        new List<IShiftTradeSwapDetail>
					                                        {
																new ShiftTradeSwapDetail(person2, person, new DateOnly(2013, 11, 02),
						                                                                 new DateOnly(2013, 11, 03)),
						                                        new ShiftTradeSwapDetail(person, person2, new DateOnly(2013, 10, 15),
						                                                                 new DateOnly(2013, 10, 16))
					                                        }));
			request.ForcePending();
			((IShiftTradeRequest)request.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByMe, new PersonRequestAuthorizationCheckerForTest());

			_target.IsSatisfiedBy(request).Should().Be(false);
		}

		private static PersonRequest createShiftTrade(DateOnly startDate, DateOnly endDate)
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			return new PersonRequest(person1, new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(person1, person2, startDate, endDate)
				}));
		}
	}
}
