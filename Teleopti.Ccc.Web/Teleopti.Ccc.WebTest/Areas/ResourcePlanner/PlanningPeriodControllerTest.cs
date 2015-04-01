using System;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class PlanningPeriodControllerTest
	{
		[Test]
		public void ShouldReturnDefaultPlanningPeriodIfNoPeriodExists()
		{
			var planningPeriodRepository = MockRepository.GenerateMock<IRepository<IPlanningPeriod>>();
			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new IPlanningPeriod[] {});

			var target = new PlanningPeriodController(planningPeriodRepository, new TestableNow(new DateTime(2015,4,1)));
			
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) target.GetPlanningPeriod();
			result.Content.StartDate.Should().Be(new DateTime(2015, 05, 01));
			result.Content.EndDate.Should().Be(new DateTime(2015, 05, 31));
		}

		[Test]
		public void ShouldSaveDefaultPlanningPeriodIfNoPeriodExists()
		{
			var planningPeriodRepository = MockRepository.GenerateMock<IRepository<IPlanningPeriod>>();
			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new IPlanningPeriod[] { });
			planningPeriodRepository.Stub(x => x.Add(null)).IgnoreArguments().Callback<IPlanningPeriod>(y =>
			{
				y.SetId(Guid.NewGuid());
				return true;
			});
			var target = new PlanningPeriodController(planningPeriodRepository, new TestableNow(new DateTime(2015, 4, 1)));

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)target.GetPlanningPeriod();
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);

			planningPeriodRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}
	}
}
