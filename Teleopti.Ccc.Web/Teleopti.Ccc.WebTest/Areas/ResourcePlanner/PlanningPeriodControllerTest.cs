using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using NUnit.Framework;
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
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldSaveNewDefaultPlanningPeriodIfPreviousStarted()
		{
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) Target.GetPlanningPeriod();
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));

			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldReturnEmptyIfForecastIsAvailable()
		{
			((FakeMissingForecastProvider) FakeMissingForecastProvider).MissingForecast = new MissingForecastModel[] {};
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) Target.GetPlanningPeriod();
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
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>) Target.GetPlanningPeriod();
			result.Content.Skills.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldGetPlanningPeriodSuggestions()
		{
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).CustomData(null, new PlanningPeriodSuggestions(TestableNow, suggestions()));
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
				new PlanningPeriod(new PlanningPeriodSuggestions(LocalTestableNow, suggestions())),
				new PlanningPeriodSuggestions(LocalTestableNow, suggestions()));


			var result =
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestion(Guid.NewGuid());
			result.Content.Where(x => x.StartDate < TestableNow.LocalDateTime()).ToList().Should().Be.Empty();
		}

		[Test]
		public void ShouldGetSortedPlanningPeriodSuggestions()
		{
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).CustomData(null, new PlanningPeriodSuggestions(TestableNow, suggestions()));
			var result = (OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>) Target.GetPlanningPeriodSuggestion(Guid.NewGuid());
			result.Content.First().StartDate.Should().Be.EqualTo(new DateTime(2015, 04, 27));
			result.Content.First().EndDate.Should().Be.EqualTo(new DateTime(2015, 05, 24));
		}

		[Test]
		public void ShouldReturnSuggestionsForNextPlanningPeriod()
		{
			changeNowTo(new DateTime(2015, 05, 23));
			var aggSchedulePeriod = new AggregatedSchedulePeriod()
			{
				Number = 2,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 10
			};
			var aggSchedulePeriod2 = new AggregatedSchedulePeriod()
			{
				Number = 1,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 6
			};
			var suggestion = new PlanningPeriodSuggestions(LocalTestableNow, new List<AggregatedSchedulePeriod>()
			{
				aggSchedulePeriod,aggSchedulePeriod2
			});
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			((FakePlanningPeriodRepository)FakePlanningPeriodRepository).CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(LocalTestableNow,
					new List<AggregatedSchedulePeriod>() { aggSchedulePeriod, aggSchedulePeriod2 })), suggestion
				);

			var result = (OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)Target.GetNextPlanningPeriodSuggestions();
			result.Content.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldCreateAndReturnNextPlanningPeriod()
		{
			changeNowTo(new DateTime(2015, 05, 23));
			var aggSchedulePeriod = new AggregatedSchedulePeriod()
			{
				Number = 2,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 10
			};
			var suggestion = new PlanningPeriodSuggestions(LocalTestableNow, new List<AggregatedSchedulePeriod>()
			{
				aggSchedulePeriod
			});
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			((FakePlanningPeriodRepository) FakePlanningPeriodRepository).CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(LocalTestableNow,
					new List<AggregatedSchedulePeriod>() {aggSchedulePeriod})), suggestion
				);


			var result =
				(OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetNextPlanningPeriod(new PlanningPeriodChangeRangeModel() { DateFrom = new DateTime(2015, 06, 15), Number = 2, PeriodType = SchedulePeriodType.Week});
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 06, 15));
			result.Content.EndDate.Should().Be.EqualTo(new DateTime(2015, 06, 28));
		}

		[Test]
		public void ShouldReturnNextPlanningPeriodIfExists()
		{
			changeNowTo(new DateTime(2015, 05, 23));
			var aggSchedulePeriod = new AggregatedSchedulePeriod()
			{
				Number = 2,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 10
			};
			var suggestion = new PlanningPeriodSuggestions(LocalTestableNow, new List<AggregatedSchedulePeriod>()
			{
				aggSchedulePeriod
			});
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(),new DateOnlyPeriod(new DateOnly(2015,05,18),new DateOnly(2015,05,31) )));
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(),new DateOnlyPeriod(new DateOnly(2015,06,01),new DateOnly(2015,06,14) )));
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(),new DateOnlyPeriod(new DateOnly(2015,06,15),new DateOnly(2015,06,28) )));
			((FakePlanningPeriodRepository)FakePlanningPeriodRepository).CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(LocalTestableNow,
					new List<AggregatedSchedulePeriod>() { aggSchedulePeriod })), suggestion
				);

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetNextPlanningPeriod(new PlanningPeriodChangeRangeModel() { DateFrom = new DateTime(2015, 06, 15), Number = 2, PeriodType = SchedulePeriodType.Week });
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 06, 15));
			result.Content.EndDate.Should().Be.EqualTo(new DateTime(2015, 06, 28));
		}

		[Test]
		public void ShouldReturnDefaultPlanningPeriodIfNotCreated()
		{
			var result = (OkNegotiatedContentResult<IEnumerable<PlanningPeriodModel>>)Target.GetAvailablePlanningPeriods();
			result.Content.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnAvailablePlanningPeriods()
		{
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 05, 18), new DateOnly(2015, 05, 31))));
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28))));

			var result = (OkNegotiatedContentResult<IEnumerable<PlanningPeriodModel>>)Target.GetAvailablePlanningPeriods();
			result.Content.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldReturnNoSuggestionsIfNextPlanningPeriodExists()
		{
			changeNowTo(new DateTime(2015, 05, 23));
			var aggSchedulePeriod = new AggregatedSchedulePeriod()
			{
				Number = 2,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 10
			};
			var aggSchedulePeriod2 = new AggregatedSchedulePeriod()
			{
				Number = 1,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 6
			};
			var suggestion = new PlanningPeriodSuggestions(LocalTestableNow, new List<AggregatedSchedulePeriod>()
			{
				aggSchedulePeriod,aggSchedulePeriod2
			});
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			FakePlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28))));
			((FakePlanningPeriodRepository)FakePlanningPeriodRepository).CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(LocalTestableNow,
					new List<AggregatedSchedulePeriod>() { aggSchedulePeriod, aggSchedulePeriod2 })), suggestion
				);

			IHttpActionResult result = Target.GetNextPlanningPeriodSuggestions();
			result.Should().Be.OfType<NotFoundResult>();
		}

		private void changeNowTo(DateTime dateTime)
		{
			((TestableNow)LocalTestableNow).CustomNow = dateTime;
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
