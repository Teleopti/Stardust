using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
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
		public INow MutableNow;
		public IMissingForecastProvider FakeMissingForecastProvider;
		public IPlanningPeriodRepository FakePlanningPeriodRepository;

		[Test]
		public void ShouldReturnDefaultPlanningPeriodIfNoPeriodExists()
		{
			((MutableNow)MutableNow).Is(new DateTime(2015, 4, 1));
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.StartDate.Should().Be(new DateTime(2015, 05, 01));
			result.Content.EndDate.Should().Be(new DateTime(2015, 05, 31));
		}

		[Test]
		public void ShouldSaveDefaultPlanningPeriodIfNoPeriodExists()
		{
			Target.GetAllPlanningPeriods();
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldSaveNewDefaultPlanningPeriodIfPreviousStarted()
		{
			((MutableNow)MutableNow).Is(new DateTime(2015, 4, 1));
			Target.GetAllPlanningPeriods();
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));

			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldReturnEmptyIfForecastIsAvailable()
		{
			((FakeMissingForecastProvider) FakeMissingForecastProvider).MissingForecast = new MissingForecastModel[] {};
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.Skills.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingForecastWhenIncompleteForecasting()
		{
			((FakeMissingForecastProvider) FakeMissingForecastProvider).MissingForecast = new[]
			{
				new MissingForecastModel
				{
					SkillName = "Direct Sales",
					MissingRanges =
						new[] {new MissingForecastRange {StartDate = new DateTime(2015, 5, 10), EndDate = new DateTime(2015, 5, 16)}}
				}
			};
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.Skills.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldGetPlanningPeriodSuggestions()
		{
			((MutableNow)MutableNow).Is(new DateTime(2015, 4, 1));
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).CustomData(null, new PlanningPeriodSuggestions(MutableNow, suggestions()));
			var result = 
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestion(Guid.NewGuid());
			result.Content.Count().Should().Be.EqualTo(15);
		}


		[Test]
		public void ShouldOnlyGetFuturePlanningPeriodSuggestions()
		{
			changeNowTo(new DateTime(2015, 05, 22));
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(MutableNow, suggestions())),
				new PlanningPeriodSuggestions(MutableNow, suggestions()));


			var result =
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestion(Guid.NewGuid());
			result.Content.Where(x => x.StartDate < MutableNow.LocalDateTime()).ToList().Should().Be.Empty();
		}

		[Test]
		public void ShouldGetSortedPlanningPeriodSuggestions()
		{
			changeNowTo(new DateTime(2015, 4, 1));
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).CustomData(null, new PlanningPeriodSuggestions(MutableNow, suggestions()));
			var result = (OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>) Target.GetPlanningPeriodSuggestion(Guid.NewGuid());
			result.Content.First().StartDate.Should().Be.EqualTo(new DateTime(2015, 04, 27));
			result.Content.First().EndDate.Should().Be.EqualTo(new DateTime(2015, 05, 24));
		}

		[Test]
		public void ShouldReturnDefaultPlanningPeriodIfNotCreated()
		{
			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods();
			result.Content.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnAvailablePlanningPeriods()
		{
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 05, 18), new DateOnly(2015, 05, 31))));
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28))));

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods();
			result.Content.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldReturnNextPlanningPeriod()
		{
			changeNowTo(new DateTime(2015, 05, 23));
			FakePlanningPeriodRepository.Add( new PlanningPeriod(new PlanningPeriodSuggestions(MutableNow,new List<AggregatedSchedulePeriod>())));

			Target.Request = new HttpRequestMessage();
			Target.GetNextPlanningPeriod();
			FakePlanningPeriodRepository.LoadAll().Any(p => p.Range.StartDate == new DateOnly(2015, 07, 01)).Should().Be.True();
		}

		[Test]
		public void ShouldReturnIndicationIfNextPlanningPeriodExists()
		{
			changeNowTo(new DateTime(2015, 05, 23));
			var currentPlanningPeriodId = Guid.NewGuid();
			var nextPlanningPeriodId = Guid.NewGuid();

			var aggSchedulePeriod = new AggregatedSchedulePeriod
			{
				Number = 2,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 10
			};
			var suggestion = new PlanningPeriodSuggestions(MutableNow, new List<AggregatedSchedulePeriod>
			{
				aggSchedulePeriod
			});
			
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(currentPlanningPeriodId, new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(nextPlanningPeriodId, new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28))));
			((FakePlanningPeriodRepository)FakePlanningPeriodRepository).CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(MutableNow,
					new List<AggregatedSchedulePeriod> { aggSchedulePeriod })), suggestion
				);

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(currentPlanningPeriodId);
			result.Content.HasNextPlanningPeriod.Should().Be(true);
		}

		private void changeNowTo(DateTime dateTime)
		{
			((MutableNow) MutableNow).Is( dateTime);
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
					Priority = 12
				},
				new AggregatedSchedulePeriod
				{
					Number = 1,
					Culture = 1053,
					DateFrom = new DateTime(2011, 01, 03),
					PeriodType = SchedulePeriodType.Week,
					Priority = 20
				}
			};
		}
	}
}
