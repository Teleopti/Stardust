using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;


namespace Teleopti.Ccc.WebTest.Core.SeatPlanner.Provider
{
	[TestFixture]
	class SeatPlanProviderTest
	{
		private FakeSeatPlanRepository _seatPlanRepository;
		private SeatPlanProvider _target;

		[SetUp]
		public void Setup()
		{
			
			_seatPlanRepository = new FakeSeatPlanRepository();
			_target = new SeatPlanProvider(_seatPlanRepository);
			addSeatPlansToRepository();
		}

		[Test]
		public void ShouldGetSeatPlan()
		{
			var date = new DateOnly (2015, 03, 02);
			var result = _target.Get(date);
			result.Date.Should().Be.EqualTo(date.Date);
		}


		[Test]
		public void ShouldGetSeatPlans()
		{
			var startDate = new DateOnly (2015, 03, 02);
			var endDate = new DateOnly (2015, 03, 03);

			var dateOnlyPeriod = new DateOnlyPeriod (startDate, endDate);
			var result = _target.Get(dateOnlyPeriod);
			result.Count.Should().Be (2);
			result[0].Date.Should().Be.EqualTo(startDate.Date);
			result[1].Date.Should().Be.EqualTo(endDate.Date);
		}


		private void addSeatPlansToRepository()
		{
			var seatPlanList = new List<SeatPlan>
			{
				createSeatPlan (new DateOnly (2015, 03, 02), SeatPlanStatus.InProgress),
				createSeatPlan (new DateOnly (2015, 03, 01), SeatPlanStatus.Ok),
				createSeatPlan (new DateOnly (2015, 03, 03), SeatPlanStatus.InError)
			};

			_seatPlanRepository.AddRange (seatPlanList);
			
		}

		private static SeatPlan createSeatPlan (DateOnly date, SeatPlanStatus status)
		{
			var seatPlan = new SeatPlan()
			{
				Date = date,
				Status = status
			};
			seatPlan.SetId (Guid.NewGuid());
			
			return seatPlan;
		}
	}
}


