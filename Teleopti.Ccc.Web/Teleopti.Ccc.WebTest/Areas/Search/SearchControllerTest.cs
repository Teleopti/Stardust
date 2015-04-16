using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	public class SearchControllerTest
	{

		[Test, SetCulture("en"), SetUICulture("en")]
		public void ShouldSearchForPlanningPeriod()
		{
			var target = new SearchController(new FakeNextPlanningPeriodProvider(new PlanningPeriod(new TestableNow(new DateTime(2015,04,15)))), new FakeToggleManager(Toggles.Wfm_ResourcePlanner_32892));
			var result = (OkNegotiatedContentResult<IEnumerable<SearchResultModel>>)target.GetResult("Next");
			result.Content.Count().Should().Be.EqualTo(1);
		}

		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldNotSearchPlanningPeriodIfSchedulingIsDisabled()
		{
			var target = new SearchController(new FakeNextPlanningPeriodProvider(new PlanningPeriod(new TestableNow(new DateTime(2015, 04, 15)))), new FakeToggleManager());
			var result = (OkNegotiatedContentResult<IEnumerable<SearchResultModel>>)target.GetResult("Next");
			result.Content.Count().Should().Be.EqualTo(0);
		}
	}

	public class FakeNextPlanningPeriodProvider : INextPlanningPeriodProvider
	{
		private readonly IPlanningPeriod _planningPeriod;

		public FakeNextPlanningPeriodProvider(IPlanningPeriod planningPeriod)
		{
			_planningPeriod = planningPeriod;
		}
		public IPlanningPeriod Current()
		{
			return _planningPeriod;
		}
	}
}
