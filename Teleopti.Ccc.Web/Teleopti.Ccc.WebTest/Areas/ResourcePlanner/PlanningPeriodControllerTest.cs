using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[ResourcePlannerTest]
	public class PlanningPeriodControllerTest
	{
		public PlanningPeriodController Target;
		public INow TestableNow;
		public IMissingForecastProvider FakeMissingForecastProvider;
		public IPlanningPeriodRepository FakePlanningPeriodRepository;
		public INow LocalTestableNow;

		[Test]
		public void ShouldReturnDefaultPlanningPeriodIfNoPeriodExists()
		{
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) Target.GetPlanningPeriod();
			result.Content.StartDate.Should().Be(new DateTime(2015, 05, 01));
			result.Content.EndDate.Should().Be(new DateTime(2015, 05, 31));
		}

		[Test]
		public void ShouldSaveDefaultPlanningPeriodIfNoPeriodExists()
		{
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) Target.GetPlanningPeriod();
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			((FakePlanningPeriodRepository)FakePlanningPeriodRepository).AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldSaveNewDefaultPlanningPeriodIfPreviousStarted()
		{
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod();
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));

			((FakePlanningPeriodRepository)FakePlanningPeriodRepository).AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldReturnEmptyIfForecastIsAvailable()
		{
			((FakeMissingForecastProvider)FakeMissingForecastProvider).MissingForecast  = new MissingForecastModel[]{};
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) Target.GetPlanningPeriod();
			result.Content.Skills.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingForecastWhenIncompleteForecasting()
		{
			((FakeMissingForecastProvider) FakeMissingForecastProvider).MissingForecast = new []{  new MissingForecastModel
			{
				SkillName = "Direct Sales",
				MissingRanges =
					new[] {new MissingForecastRange {StartDate = new DateTime(2015, 5, 10), EndDate = new DateTime(2015, 5, 16)}}
			}};
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) Target.GetPlanningPeriod();
			result.Content.Skills.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldGetPlanningPeriodSuggestions()
		{
			Guid id = Guid.NewGuid();
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).CustomSuggestions =
				new PlanningPeriodSuggestions(TestableNow, suggestions());

			var result = (OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)Target.GetPlanningPeriodSuggestion(id);
			result.Content.Count().Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldGetPlanningPeriodSuggestionsInFuture()
		{
			((TestableNow)LocalTestableNow).CustomNow = new DateTime(2015, 05, 22);
			Guid id = Guid.NewGuid();
			((FakePlanningPeriodRepository)FakePlanningPeriodRepository).CustomPlanningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(LocalTestableNow, suggestions()));
			((FakePlanningPeriodRepository)FakePlanningPeriodRepository).CustomSuggestions =
				new PlanningPeriodSuggestions(LocalTestableNow, suggestions());


			var result = (OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)Target.GetPlanningPeriodSuggestion(id);
			result.Content.Where(x=>x.StartDate>=TestableNow.LocalDateTime()).ToList().Count().Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldChangePlanningPeriodRange()
		{
			Guid id = Guid.NewGuid();
			var planningPeriodChangeRangeModel = new PlanningPeriodChangeRangeModel
			{
				Number = 1,
				PeriodType = SchedulePeriodType.Week,
				DateFrom = new DateTime(2015, 05, 01)
			};
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.ChangeRange(id, planningPeriodChangeRangeModel );
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 05, 01));
			result.Content.EndDate.Should().Be.EqualTo(new DateTime(2015, 05, 07));
		}

		private static List<AggregatedSchedulePeriod> suggestions()
		{
			return new List<AggregatedSchedulePeriod>
			{
				new AggregatedSchedulePeriod
				{
					Number = 4,
					Culture = 1053,
					DateFrom = new DateTime(2011, 05, 02),
					PeriodType = SchedulePeriodType.Week,
					Priority = 10
				},
				new AggregatedSchedulePeriod
				{
					Number = 4,
					Culture = 1053,
					DateFrom = new DateTime(2011, 05, 02),
					PeriodType = SchedulePeriodType.Week,
					Priority = 11
				},
				new AggregatedSchedulePeriod
				{
					Number = 1,
					Culture = 1053,
					DateFrom = new DateTime(2011, 01, 03),
					PeriodType = SchedulePeriodType.Week,
					Priority = 11
				}
			};
		}
	}
}
