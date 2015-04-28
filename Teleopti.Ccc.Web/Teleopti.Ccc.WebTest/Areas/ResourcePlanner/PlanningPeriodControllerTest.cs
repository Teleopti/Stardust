using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class PlanningPeriodControllerTest
	{
		[Test]
		public void ShouldReturnDefaultPlanningPeriodIfNoPeriodExists()
		{
			var fakeNow = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();
			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new IPlanningPeriod[] {});
			planningPeriodRepository.Stub(x => x.Suggestions(fakeNow)).Return(new PlanningPeriodSuggestions(fakeNow, new List<AggregatedSchedulePeriod>() {}));

			var target = new PlanningPeriodController(new NextPlanningPeriodProvider(planningPeriodRepository, fakeNow,new FakeToggleManager(Toggles.Wfm_ChangePlanningPeriod_33043)), new FakeMissingForecastProvider());
			
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) target.GetPlanningPeriod();
			result.Content.StartDate.Should().Be(new DateTime(2015, 05, 01));
			result.Content.EndDate.Should().Be(new DateTime(2015, 05, 31));
		}

		[Test]
		public void ShouldSaveDefaultPlanningPeriodIfNoPeriodExists()
		{
			var fakeNow = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();

			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new IPlanningPeriod[] { });
			planningPeriodRepository.Stub(x => x.Suggestions(fakeNow)).Return(new PlanningPeriodSuggestions(fakeNow, new List<AggregatedSchedulePeriod>() { }));
			planningPeriodRepository.Stub(x => x.Add(null)).IgnoreArguments().Callback<IPlanningPeriod>(y =>
			{
				y.SetId(Guid.NewGuid());
				return true;
			});
			var target = new PlanningPeriodController(new NextPlanningPeriodProvider(planningPeriodRepository, fakeNow,new FakeToggleManager(Toggles.Wfm_ChangePlanningPeriod_33043)), new FakeMissingForecastProvider());

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)target.GetPlanningPeriod();
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);

			planningPeriodRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldReturnEmptyIfForecastIsAvailable()
		{
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();
			var now = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriod = new PlanningPeriod(now);

			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new[] { planningPeriod });
			planningPeriodRepository.Stub(x => x.Suggestions(now))
				.Return(new PlanningPeriodSuggestions(now,
					new List<AggregatedSchedulePeriod>()
					{
						new AggregatedSchedulePeriod()
						{
							Number = 1,
							Culture = 0,
							DateFrom = new DateTime(2014, 04, 01),
							PeriodType = SchedulePeriodType.Week,
							Priority = 10
						},
						new AggregatedSchedulePeriod()
						{
							Number = 1,
							Culture = 0,
							DateFrom = new DateTime(2014, 04, 01),
							PeriodType = SchedulePeriodType.Day,
							Priority = 11
						}
					}));

			var target = new PlanningPeriodController(new NextPlanningPeriodProvider(planningPeriodRepository, now,new FakeToggleManager(Toggles.Wfm_ChangePlanningPeriod_33043)), new FakeMissingForecastProvider());

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)target.GetPlanningPeriod();
			result.Content.Skills.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingForecastWhenIncompleteForecasting()
		{
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();
			var now = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriod = new PlanningPeriod(now);

			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new[] { planningPeriod });
			planningPeriodRepository.Stub(x => x.Suggestions(now))
				.Return(new PlanningPeriodSuggestions(now,
					new List<AggregatedSchedulePeriod>()
					{
						new AggregatedSchedulePeriod()
						{
							Number = 1,
							Culture = 0,
							DateFrom = new DateTime(2014, 04, 01),
							PeriodType = SchedulePeriodType.Week,
							Priority = 10
						},
						new AggregatedSchedulePeriod()
						{
							Number = 1,
							Culture = 0,
							DateFrom = new DateTime(2014, 04, 01),
							PeriodType = SchedulePeriodType.Day,
							Priority = 11
						}
					}));
			var target = new PlanningPeriodController(new NextPlanningPeriodProvider(planningPeriodRepository, now,new FakeToggleManager(Toggles.Wfm_ChangePlanningPeriod_33043)),
				new FakeMissingForecastProvider(new MissingForecastModel
				{
					SkillName = "Direct Sales",
					MissingRanges = new[] {new MissingForecastRange{ StartDate = new DateTime(2015, 5, 10), EndDate = new DateTime(2015, 5, 16)}}
				}));
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)target.GetPlanningPeriod();
			result.Content.Skills.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnPlanningPeriodSuggestions()
		{
			var fakeNow = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();

			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new IPlanningPeriod[] { });
			planningPeriodRepository.Stub(x => x.Suggestions(fakeNow))
				.Return(new PlanningPeriodSuggestions(fakeNow,
					new List<AggregatedSchedulePeriod>()
					{
						new AggregatedSchedulePeriod()
						{
							Number = 1,
							Culture = 0,
							DateFrom = new DateTime(2014, 04, 01),
							PeriodType = SchedulePeriodType.Week,
							Priority = 10
						},
						new AggregatedSchedulePeriod()
						{
							Number = 1,
							Culture = 0,
							DateFrom = new DateTime(2014, 04, 01),
							PeriodType = SchedulePeriodType.Day,
							Priority = 11
						}
					}));
			planningPeriodRepository.Stub(x => x.Add(null)).IgnoreArguments().Callback<IPlanningPeriod>(y =>
			{
				y.SetId(Guid.NewGuid());
				return true;
			});
			var target = new PlanningPeriodController(new NextPlanningPeriodProvider(planningPeriodRepository, fakeNow,new FakeToggleManager(Toggles.Wfm_ChangePlanningPeriod_33043)), new FakeMissingForecastProvider());

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)target.GetPlanningPeriod();
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			result.Content.SuggestedPeriods.Count().Should().Be.EqualTo(2);
		}

		//[Test]
		//public void ShouldUpdatePlanningPeriod()
		//{
		//	var planningPeriodRepository = MockRepository.GenerateMock<IRepository<IPlanningPeriod>>();
		//	var newGuid = Guid.NewGuid();
		//	var startDate = new DateTime(2015,05,01);
		//	var endDate = new DateTime(2015, 05, 31);
		//	var now = new TestableNow(new DateTime(2015, 4, 1));
			
		//	var planningPeriodModel = new PlanningPeriodModel()
		//	{
		//		Id = newGuid,
		//		StartDate = startDate,
		//		EndDate = endDate
		//	};

		//	planningPeriodRepository.Stub(x => x.Load(newGuid)).Return(new PlanningPeriod(now));
		//	var target = new PlanningPeriodController(new NextPlanningPeriodProvider(planningPeriodRepository, now), new FakeMissingForecastProvider());
		//	target.Request = new HttpRequestMessage(new HttpMethod("POST"), "/");
			

		//	target.UpdatePlanningPeriod(planningPeriodModel);
		//}
	}
}
