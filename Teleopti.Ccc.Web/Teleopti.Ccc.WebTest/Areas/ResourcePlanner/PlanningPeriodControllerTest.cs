using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
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
			planningPeriodRepository.Stub(x => x.Suggestions(fakeNow))
				.Return(new PlanningPeriodSuggestions(fakeNow, new List<AggregatedSchedulePeriod>()));

			var target =
				new PlanningPeriodController(
					new NextPlanningPeriodProvider(planningPeriodRepository, fakeNow), new FakeMissingForecastProvider(), planningPeriodRepository, fakeNow);

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) target.GetPlanningPeriod();
			result.Content.StartDate.Should().Be(new DateTime(2015, 05, 01));
			result.Content.EndDate.Should().Be(new DateTime(2015, 05, 31));
		}

		[Test]
		public void ShouldSaveDefaultPlanningPeriodIfNoPeriodExists()
		{
			var fakeNow = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();

			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new IPlanningPeriod[] {});
			planningPeriodRepository.Stub(x => x.Suggestions(fakeNow))
				.Return(new PlanningPeriodSuggestions(fakeNow, new List<AggregatedSchedulePeriod>()));
			planningPeriodRepository.Stub(x => x.Add(null)).IgnoreArguments().Callback<IPlanningPeriod>(y =>
			{
				y.SetId(Guid.NewGuid());
				return true;
			});
			var target =
				new PlanningPeriodController(
					new NextPlanningPeriodProvider(planningPeriodRepository, fakeNow), new FakeMissingForecastProvider(), planningPeriodRepository, fakeNow);

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) target.GetPlanningPeriod();
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);

			planningPeriodRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldSaveNewDefaultPlanningPeriodIfPreviousStarted()
		{
			var fakeNow = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();

			planningPeriodRepository.Stub(x => x.LoadAll())
				.Return(new IPlanningPeriod[]
				{
					new PlanningPeriod(new PlanningPeriodSuggestions(new TestableNow(new DateTime(2015, 3, 1)),
						new List<AggregatedSchedulePeriod>()))
				});
			planningPeriodRepository.Stub(x => x.Suggestions(fakeNow))
				.Return(new PlanningPeriodSuggestions(fakeNow, new List<AggregatedSchedulePeriod>()));
			planningPeriodRepository.Stub(x => x.Add(null)).IgnoreArguments().Callback<IPlanningPeriod>(y =>
			{
				y.SetId(Guid.NewGuid());
				return true;
			});
			var target =
				new PlanningPeriodController(
					new NextPlanningPeriodProvider(planningPeriodRepository, fakeNow), new FakeMissingForecastProvider(), planningPeriodRepository, fakeNow);

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)target.GetPlanningPeriod();
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));

			planningPeriodRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldReturnEmptyIfForecastIsAvailable()
		{
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();
			var now = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(now, new List<AggregatedSchedulePeriod>()));

			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new IPlanningPeriod[] {planningPeriod});
			planningPeriodRepository.Stub(x => x.Suggestions(now))
				.Return(new PlanningPeriodSuggestions(now, suggestions()));

			var target =
				new PlanningPeriodController(
					new NextPlanningPeriodProvider(planningPeriodRepository, now), new FakeMissingForecastProvider(), planningPeriodRepository, now);

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) target.GetPlanningPeriod();
			result.Content.Skills.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingForecastWhenIncompleteForecasting()
		{
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();
			var now = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(now,new List<AggregatedSchedulePeriod>()));

			planningPeriodRepository.Stub(x => x.LoadAll()).Return(new IPlanningPeriod[] {planningPeriod});
			planningPeriodRepository.Stub(x => x.Suggestions(now))
				.Return(new PlanningPeriodSuggestions(now, suggestions()));
			var target =
				new PlanningPeriodController(
					new NextPlanningPeriodProvider(planningPeriodRepository, now),
					new FakeMissingForecastProvider(new MissingForecastModel
					{
						SkillName = "Direct Sales",
						MissingRanges =
							new[] {new MissingForecastRange {StartDate = new DateTime(2015, 5, 10), EndDate = new DateTime(2015, 5, 16)}}
					}),planningPeriodRepository,now);
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) target.GetPlanningPeriod();
			result.Content.Skills.Should().Not.Be.Empty();
		}

		private static List<AggregatedSchedulePeriod> suggestions()
		{
			return new List<AggregatedSchedulePeriod>
			{
				new AggregatedSchedulePeriod
				{
					Number = 1,
					Culture = 1053,
					DateFrom = new DateTime(2015, 04, 01),
					PeriodType = SchedulePeriodType.Week,
					Priority = 10
				},
				new AggregatedSchedulePeriod
				{
					Number = 1,
					Culture = 1053,
					DateFrom = new DateTime(2015, 04, 01),
					PeriodType = SchedulePeriodType.Day,
					Priority = 11
				}
			};
		}

		[Test]
		public void ShouldGetPlanningPeriodSuggestions()
		{
			var fakeNow = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();
			Guid id = Guid.NewGuid();
			planningPeriodRepository.Stub(x => x.Load(id)).Return(new PlanningPeriod(new PlanningPeriodSuggestions(fakeNow,new List<AggregatedSchedulePeriod>())));
			planningPeriodRepository.Stub(x => x.Suggestions(fakeNow))
				.Return(new PlanningPeriodSuggestions(fakeNow,
					suggestions()));
			var target =
				new PlanningPeriodController(
					new NextPlanningPeriodProvider(planningPeriodRepository, fakeNow), new FakeMissingForecastProvider(),
					planningPeriodRepository, fakeNow);
			var result = (OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)target.GetPlanningPeriodSuggestion(id);
			result.Content.Count().Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldChangePlanningPeriodRange()
		{
			var fakeNow = new TestableNow(new DateTime(2015, 4, 1));
			var planningPeriodRepository = MockRepository.GenerateMock<IPlanningPeriodRepository>();
			Guid id = Guid.NewGuid();
			var planningPeriodChangeRangeModel = new PlanningPeriodChangeRangeModel
			{
				Number = 1,
				PeriodType = SchedulePeriodType.Week,
				DateFrom = new DateTime(2015, 05, 01)
			};
			planningPeriodRepository.Stub(x => x.Load(id)).Return(new PlanningPeriod(new PlanningPeriodSuggestions(fakeNow, new List<AggregatedSchedulePeriod>())));
			var target =
				new PlanningPeriodController(
					new NextPlanningPeriodProvider(planningPeriodRepository, fakeNow), new FakeMissingForecastProvider(),
					planningPeriodRepository, fakeNow);
			
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)target.ChangeRange(id, planningPeriodChangeRangeModel );
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 05, 01));
			result.Content.EndDate.Should().Be.EqualTo(new DateTime(2015, 05, 07));
		}
	}

	
}
