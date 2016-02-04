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
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[ResourcePlannerTest_DontUse]
	public class PlanningPeriodControllerTest
	{
		//break this test in to multiple
		public PlanningPeriodController Target;
		public MutableNow Now;
		public FakeMissingForecastProvider MissingForecastProvider;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeFixedStaffLoader FixedStaffLoader;

		[Test]
		public void ShouldReturnDefaultPlanningPeriodIfNoPeriodExists()
		{
			Now.Is(new DateTime(2015, 4, 1));
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
			PlanningPeriodRepository.AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldSaveNewDefaultPlanningPeriodIfPreviousStarted()
		{
			Now.Is(new DateTime(2015, 4, 1));
			Target.GetAllPlanningPeriods();
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));

			PlanningPeriodRepository.AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldReturnEmptyIfForecastIsAvailable()
		{
			MissingForecastProvider.MissingForecast = new MissingForecastModel[] {};
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.Skills.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingForecastWhenIncompleteForecasting()
		{
			MissingForecastProvider.MissingForecast = new[]
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
			Now.Is(new DateTime(2015, 4, 1));
			PlanningPeriodRepository.CustomData(null, new PlanningPeriodSuggestions(Now, suggestions()));
			var result = 
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestion(Guid.NewGuid());
			result.Content.Count().Should().Be.EqualTo(15);
		}
		
		[Test]
		public void ShouldOnlyGetFuturePlanningPeriodSuggestions()
		{
			Now.Is(new DateTime(2015, 05, 22));
			PlanningPeriodRepository.CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(Now, suggestions())),
				new PlanningPeriodSuggestions(Now, suggestions()));


			var result =
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestion(Guid.NewGuid());
			result.Content.Where(x => x.StartDate < Now.LocalDateTime()).ToList().Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishSchedules()
		{
			Now.Is(new DateTime(2015, 4, 1));
			PlanningPeriodRepository.CustomData(new PlanningPeriod(new PlanningPeriodSuggestions(Now, suggestions())),
				new PlanningPeriodSuggestions(Now, suggestions()));
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2010, 1, 1));
			FixedStaffLoader.SetPeople(person);

			var result = (OkResult)Target.Publish(Guid.NewGuid());
			result.Should().Not.Be.Null();

			person.WorkflowControlSet.SchedulePublishedToDate.Value.Should().Be.GreaterThan(Now.UtcDateTime());
		}

		[Test]
		public void ShouldGetSortedPlanningPeriodSuggestions()
		{
			Now.Is(new DateTime(2015, 4, 1));
			PlanningPeriodRepository.CustomData(null, new PlanningPeriodSuggestions(Now, suggestions()));
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
			PlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 05, 18), new DateOnly(2015, 05, 31))));
			PlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			PlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28))));

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods();
			result.Content.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldReturnAvailablePlanningPeriodsForRange()
		{
			PlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 05, 18), new DateOnly(2015, 05, 31))));
			PlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			PlanningPeriodRepository.Add(new FakePlanningPeriod(Guid.NewGuid(), new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28))));

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)
				Target.GetAllPlanningPeriods(new DateTime(2015, 06, 01), new DateTime(2015, 06, 16));
			result.Content.Count().Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldReturnNextPlanningPeriod()
		{
			Now.Is(new DateTime(2015, 05, 23));
			PlanningPeriodRepository.Add( new PlanningPeriod(new PlanningPeriodSuggestions(Now,new List<AggregatedSchedulePeriod>())));

			Target.Request = new HttpRequestMessage();
			Target.GetNextPlanningPeriod();
			PlanningPeriodRepository.LoadAll().Any(p => p.Range.StartDate == new DateOnly(2015, 07, 01)).Should().Be.True();
		}

		[Test]
		public void ShouldReturnIndicationIfNextPlanningPeriodExists()
		{
			Now.Is(new DateTime(2015, 05, 23));
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
			var suggestion = new PlanningPeriodSuggestions(Now, new List<AggregatedSchedulePeriod>
			{
				aggSchedulePeriod
			});
			
			PlanningPeriodRepository.Add(new FakePlanningPeriod(currentPlanningPeriodId, new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14))));
			PlanningPeriodRepository.Add(new FakePlanningPeriod(nextPlanningPeriodId, new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28))));
			PlanningPeriodRepository.CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(Now,
					new List<AggregatedSchedulePeriod> { aggSchedulePeriod })), suggestion
				);

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(currentPlanningPeriodId);
			result.Content.HasNextPlanningPeriod.Should().Be(true);
		}

		[Test]
		public void ShouldCreatePlanningPeriodWithNewState()
		{
			Now.Is(new DateTime(2015, 4, 1));
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.State.Should().Be("New");
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
					Priority = 20
				},
				new AggregatedSchedulePeriod
				{
					Number = 4,
					Culture = null,
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
					Priority = 10
				}
			};
		}
	}
}
