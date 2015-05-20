﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	public class SearchControllerTest
	{

		[Test, SetCulture("en"), SetUICulture("en")]
		public void ShouldSearchForPlanningPeriod()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var newIdentity = new TeleoptiIdentity("test2", null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);
			var target =
				new SearchController(
					new FakeNextPlanningPeriodProvider(
						new PlanningPeriod(
							(new PlanningPeriodSuggestions(new TestableNow(new DateTime(2015, 4, 15)), new List<AggregatedSchedulePeriod>())))),
					new FakeToggleManager(Toggles.Wfm_ResourcePlanner_32892));
			var result = (OkNegotiatedContentResult<IEnumerable<SearchResultModel>>)target.GetResult("Next");
			result.Content.Count().Should().Be.EqualTo(1);
		}

		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldNotSearchPlanningPeriodIfSchedulingIsDisabled()
		{
			var target =
				new SearchController(
					new FakeNextPlanningPeriodProvider(
						new PlanningPeriod(
							(new PlanningPeriodSuggestions(new TestableNow(new DateTime(2015, 4, 15)), new List<AggregatedSchedulePeriod>())))),
					new FakeToggleManager());
			var result = (OkNegotiatedContentResult<IEnumerable<SearchResultModel>>)target.GetResult("Next");
			result.Content.Count().Should().Be.EqualTo(0);
		}
	}

	//may be remove this fake (no fake at this level)
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

		public IPlanningPeriod Find(Guid id)
		{
			return _planningPeriod;
		}

		public IEnumerable<SchedulePeriodType> SuggestedPeriods()
		{
			throw new NotImplementedException();
		}

		public IPlanningPeriod Update(SchedulePeriodType periodType, int periodRange)
		{
			throw new NotImplementedException();
		}
	}
}
