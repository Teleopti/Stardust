﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[DomainTest]
	public class PlanningPeriodControllerTest : IExtendSystem
	{
		//break this test in to multiple
		public PlanningPeriodController Target;
		public MutableNow Now;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeExistingForecastRepository ExistingForecastRepository;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldNotCreateDefaultPlanningPeriodForPlanningGroupIfNonExists()
		{
			var planningGroupId = Guid.NewGuid();
			var planningGroup = new PlanningGroup("test group").WithId(planningGroupId);
			PlanningGroupRepository.Add(planningGroup);

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods(planningGroupId);
			result.Content.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnAvailablePlanningPeriodsForPlanningGroup()
		{
			var planningGroupId = Guid.NewGuid();
			var planningGroup = new PlanningGroup("test group").WithId(planningGroupId);
			PlanningGroupRepository.Add(planningGroup);

			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 05, 18), new DateOnly(2015, 05, 31)),SchedulePeriodType.Week, 2, planningGroup).WithId());
			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14)),SchedulePeriodType.Week, 2,planningGroup).WithId());
			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28)), SchedulePeriodType.Week, 2, planningGroup).WithId());


			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods(planningGroupId);
			result.Content.Count.Should().Be.EqualTo(3);
			result.Content.ForEach(x => x.PlanningGroupId.Should().Be.EqualTo(planningGroupId));
		}

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
			var periods = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods();
			
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(periods.Content.First().Id);
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			PlanningPeriodRepository.AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldSaveNewDefaultPlanningPeriodIfPreviousStarted()
		{
			Now.Is(new DateTime(2015, 4, 1));
			var periods = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods();
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(periods.Content.First().Id);
			result.Content.Id.Should().Not.Be.EqualTo(Guid.Empty);
			result.Content.StartDate.Should().Be.EqualTo(new DateTime(2015, 5, 1));

			PlanningPeriodRepository.AddExecuted.Should().Be.True();
		}

		[Test]
		public void ShouldReturnEmptyIfForecastIsAvailable()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			var skill = SkillFactory.CreateSkill("Direct Sales", new SkillTypePhone(new Description("Phone"), ForecastSource.InboundTelephony), 15).WithId();
			var planningGroup = new PlanningGroup().WithId().AddFilter(new SkillFilter(skill));
			PlanningGroupRepository.Has(planningGroup);
			var personPeriodStart = new DateOnly(2015, 5, 10);
			var planningPeriod = PlanningPeriodRepository.Has(personPeriodStart, 1, planningGroup);
			var person = PersonFactory.CreatePersonWithPersonPeriod(personPeriodStart, new[] { skill }).WithName(new Name("Tester", "Testersson")).WithId();
			person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(personPeriodStart));
			person.Period(personPeriodStart).RuleSetBag = new RuleSetBag();
			PersonRepository.Has(person);
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>();

			var result = (OkNegotiatedContentResult<HintResult>)Target.GetValidation(planningPeriod.Id.GetValueOrDefault());

			result.Content.InvalidResources.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingForecastWhenIncompleteForecasting()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			var skill = SkillFactory.CreateSkill("Direct Sales", new SkillTypePhone(new Description("Phone"), ForecastSource.InboundTelephony), 15).WithId();
			var planningGroup = new PlanningGroup().WithId().AddFilter(new SkillFilter(skill));
			PlanningGroupRepository.Has(planningGroup);
			var personPeriodStart = new DateOnly(2015, 5, 10);
			var planningPeriod = PlanningPeriodRepository.Has(personPeriodStart, 1, planningGroup);
			var person = PersonFactory.CreatePersonWithPersonPeriod(personPeriodStart, new[] { skill }).WithName(new Name("Tester", "Testersson")).WithId();
			person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(personPeriodStart));
			person.Period(personPeriodStart).RuleSetBag = new RuleSetBag();
			PersonRepository.Has(person);
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>
			{
				new SkillMissingForecast
				{
					Periods = new[]{new DateOnlyPeriod(new DateOnly(2015, 4, 1), new DateOnly(2015, 4, 30)) },
					SkillName = skill.Name,
					SkillId = skill.Id.GetValueOrDefault()
				}
			};

			var result = (OkNegotiatedContentResult<HintResult>)Target.GetValidation(planningPeriod.Id.GetValueOrDefault());

			result.Content.InvalidResources.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldGetPlanningPeriodSuggestions()
		{
			Now.Is(new DateTime(2015, 4, 1));
			PlanningPeriodRepository.CustomData(null, new PlanningPeriodSuggestions(Now, suggestions()));
			var result = 
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestion(Guid.NewGuid());
			result.Content.Count().Should().Be.EqualTo(14);
		}
		
		[Test]
		public void ShouldOnlyGetFuturePlanningPeriodSuggestions()
		{
			Now.Is(new DateTime(2015, 05, 22));
			var planningPeriodId = Guid.NewGuid();
			PlanningPeriodRepository.CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(Now, suggestions())).WithId(planningPeriodId),
				new PlanningPeriodSuggestions(Now, suggestions()));


			var result =
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestion(planningPeriodId);
			result.Content.Where(x => x.StartDate < Now.ServerDateTime_DontUse()).ToList().Should().Be.Empty();
		}

		[Test]
		public void ShouldGetPlanningPeriodSuggestionsForPlanningGroup()
		{
			Now.Is(new DateTime(2015, 4, 1));
			var planningGroup = new PlanningGroup().WithId(Guid.NewGuid());
			PlanningGroupRepository.Has(planningGroup);
			PlanningPeriodRepository.CustomData(null, new PlanningPeriodSuggestions(Now, suggestions()));
			var result =
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestionsForPlanningGroup(planningGroup.Id.GetValueOrDefault());
			result.Content.Count().Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldOnlyGetFuturePlanningPeriodSuggestionsForPlanningGroup()
		{
			Now.Is(new DateTime(2015, 05, 22));
			var planningGroup = new PlanningGroup().WithId(Guid.NewGuid());
			PlanningGroupRepository.Has(planningGroup);
			var planningPeriodId = Guid.NewGuid();
			PlanningPeriodRepository.CustomData(
				new PlanningPeriod(new PlanningPeriodSuggestions(Now, suggestions())).WithId(planningPeriodId),
				new PlanningPeriodSuggestions(Now, suggestions()));


			var result =
				(OkNegotiatedContentResult<IEnumerable<SuggestedPlanningPeriodRangeModel>>)
					Target.GetPlanningPeriodSuggestionsForPlanningGroup(planningGroup.Id.GetValueOrDefault());
			result.Content.Where(x => x.StartDate < Now.ServerDateTime_DontUse()).ToList().Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishSchedules()
		{
			Now.Is(new DateTime(2015, 4, 1));
			var planningPeriodId = Guid.NewGuid();
			PlanningPeriodRepository.CustomData(new PlanningPeriod(new PlanningPeriodSuggestions(Now, suggestions())).WithId(planningPeriodId),
				new PlanningPeriodSuggestions(Now, suggestions()));
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2010, 1, 1));
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1)));
			PersonRepository.Has(person);

			var result = (OkResult)Target.Publish(planningPeriodId);
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
			result.Content.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnAvailablePlanningPeriods()
		{
			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 05, 18), new DateOnly(2015, 05, 31)), SchedulePeriodType.Week, 2).WithId());
			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14)), SchedulePeriodType.Week, 2).WithId());
			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28)), SchedulePeriodType.Week, 2).WithId());

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods();
			result.Content.Count.Should().Be.EqualTo(3);
			result.Content.ForEach(x=>x.PlanningGroupId.Should().Be.EqualTo(null));
		}

		[Test]
		public void ShouldReturnAvailablePlanningPeriodsForRange()
		{
			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 05, 18), new DateOnly(2015, 05, 31)), SchedulePeriodType.Week, 2).WithId());
			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 06, 01), new DateOnly(2015, 06, 14)), SchedulePeriodType.Week, 2).WithId());
			PlanningPeriodRepository.Add(new PlanningPeriod(new DateOnlyPeriod(new DateOnly(2015, 06, 15), new DateOnly(2015, 06, 28)), SchedulePeriodType.Week, 2).WithId());

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)
				Target.GetAllPlanningPeriods(new DateTime(2015, 06, 01), new DateTime(2015, 06, 16));
			result.Content.Count.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldReturnNextPlanningPeriod()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>();
			Now.Is(new DateTime(2015, 05, 23));
			PlanningPeriodRepository.Add( new PlanningPeriod(new PlanningPeriodSuggestions(Now,new List<AggregatedSchedulePeriod>())));

			Target.Request = new HttpRequestMessage();
			Target.GetNextPlanningPeriod();
			PlanningPeriodRepository.LoadAll().Any(p => p.Range.StartDate == new DateOnly(2015, 07, 01)).Should().Be.True();
		}

		[Test]
		public void ShouldReturnNextPlanningPeriodForPlanningGroup()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>();
			var planningGroupId = Guid.NewGuid();
			var planningGroup = new PlanningGroup().WithId(planningGroupId);
			PlanningGroupRepository.Add(planningGroup);
			Now.Is(new DateTime(2015, 05, 23));
			PlanningPeriodRepository.Add(new PlanningPeriod(new PlanningPeriodSuggestions(Now, new List<AggregatedSchedulePeriod>()), planningGroup));

			Target.Request = new HttpRequestMessage();
			Target.GetNextPlanningPeriod(planningGroupId);
			PlanningPeriodRepository.LoadAll()
				.Any(p => p.Range.StartDate == new DateOnly(2015, 07, 01) && p.PlanningGroup.Id.Value == planningGroupId)
				.Should()
				.Be.True();
		}

		[Test]
		public void ShouldReturnIndicationIfNextPlanningPeriodExists()
		{
			var currentPeriod = PlanningPeriodRepository.Has(new DateOnly(2015, 06, 01), 2);
			PlanningPeriodRepository.Has(new DateOnly(2015, 06, 15), 2);

			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(currentPeriod.Id.GetValueOrDefault());
			result.Content.HasNextPlanningPeriod.Should().Be(true);
		}

		[Test]
		public void ShouldCreatePlanningPeriodWithNewState()
		{
			Now.Is(new DateTime(2015, 4, 1));
			var result = (OkNegotiatedContentResult<PlanningPeriodModel>)Target.GetPlanningPeriod(Guid.NewGuid());
			result.Content.State.Should().Be("New");
		}

		[Test]
		public void ShouldRemoveLatestPlanningPeriodForAgent()
		{
			var planningGroup = new PlanningGroup()
				.WithId();
			PlanningGroupRepository.Has(planningGroup);
			PlanningPeriodRepository.Has(new DateOnly(2017, 04, 19), 1, planningGroup);
			PlanningPeriodRepository.Has(new DateOnly(2017, 04, 26), 1, planningGroup);

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.DeleteLastPeriod(planningGroup.Id.GetValueOrDefault());

			var planningPeriods = PlanningPeriodRepository.LoadForPlanningGroup(planningGroup).ToList();
			planningPeriods.SingleOrDefault()?.Id.Should().Be.EqualTo(result.Content.SingleOrDefault()?.Id);
		}

		[Test]
		public void ShouldChangeLastPeriodAndCreateNewPeriodsWithSameRange()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			var planningGroupId = Guid.NewGuid();
			var planningGroup = new PlanningGroup()
				.WithId(planningGroupId);
			PlanningGroupRepository.Has(planningGroup);
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>();

			var firstPeriod = PlanningPeriodRepository.Has(new DateOnly(2017, 04, 19), 1, planningGroup);
			var periodStart = new DateOnly(2017, 04, 26);
			var changedPeriod = PlanningPeriodRepository.Has(periodStart, 1, planningGroup);

			Target.ChangeLastPeriod(planningGroupId, periodStart.Date + TimeSpan.FromDays(14), SchedulePeriodType.Day, 15);

			Target.Request = new HttpRequestMessage();
			var lastPeriod = (CreatedAtRouteNegotiatedContentResult<PlanningPeriodModel>) Target.GetNextPlanningPeriod(planningGroupId);
			var periods = PlanningPeriodRepository.LoadForPlanningGroup(planningGroup).OrderBy(period => period.Range.StartDate);

			periods.First(period => period.Id == firstPeriod.Id).Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 04, 25));
			periods.First(period => period.Id == changedPeriod.Id).Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 05, 10));
			periods.First(period => period.Id == lastPeriod.Content.Id).Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 05, 25));
		}

		[Test]
		public void ShouldNotBeAbleToChangeStartDateOfSecondPeriod()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			var planningGroupId = Guid.NewGuid();
			var planningGroup = new PlanningGroup()
				.WithId(planningGroupId);
			PlanningGroupRepository.Has(planningGroup);
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>();

			PlanningPeriodRepository.Has(new DateOnly(2017, 04, 19), 1, planningGroup);
			var periodStart = new DateOnly(2017, 04, 26);
			PlanningPeriodRepository.Has(periodStart, 1, planningGroup);

			var result = Target.ChangeLastPeriod(planningGroupId, periodStart.Date + TimeSpan.FromDays(14), SchedulePeriodType.Week, 2, periodStart.Date + TimeSpan.FromDays(1));
			(result as BadRequestErrorMessageResult).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldBeAbleToChangeStartDateOfFirstPeriod()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			var planningGroupId = Guid.NewGuid();
			var planningGroup = new PlanningGroup()
				.WithId(planningGroupId);
			PlanningGroupRepository.Has(planningGroup);
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>();

			var periodStart = new DateOnly(2017, 04, 26);
			PlanningPeriodRepository.Has(periodStart, 1, planningGroup);

			var newPeriodStart = periodStart.Date + TimeSpan.FromDays(1);
			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.ChangeLastPeriod(planningGroupId, newPeriodStart.Date + TimeSpan.FromDays(14), SchedulePeriodType.Day, 15,newPeriodStart);
			result.Content.First().StartDate.Should().Be.EqualTo(newPeriodStart);
		}

		[Test]
		public void ShouldBeStatusFailedWhenSchedulingFailedLastJob()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			var planningGroupId = Guid.NewGuid();
			var planningGroup = new PlanningGroup()
				.WithId(planningGroupId);
			PlanningGroupRepository.Has(planningGroup);
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>();

			var periodStart = new DateOnly(2017, 04, 26);
			var planningPeriod = PlanningPeriodRepository.Has(periodStart, 1, planningGroup);
			var jobResult = new JobResult(JobCategory.WebSchedule, planningPeriod.Range, PersonFactory.CreatePerson(), DateTime.UtcNow);
			jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, "Whatever", DateTime.Now, new Exception()));
			planningPeriod.JobResults.Add(jobResult);

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods(planningGroupId);
			result.Content.First().State.Should().Be(PlanningPeriodState.ScheduleFailed.ToString());
		}

		[Test]
		public void ShouldBeStatusFailedWhenIntradayOptimizationFailedLastJob()
		{
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("Default", true, true).WithId());
			var planningGroupId = Guid.NewGuid();
			var planningGroup = new PlanningGroup()
				.WithId(planningGroupId);
			PlanningGroupRepository.Has(planningGroup);
			ExistingForecastRepository.CustomResult = new List<SkillMissingForecast>();

			var periodStart = new DateOnly(2017, 04, 26);
			var planningPeriod = PlanningPeriodRepository.Has(periodStart, 1, planningGroup);
			var jobResult = new JobResult(JobCategory.WebIntradayOptimization, planningPeriod.Range, PersonFactory.CreatePerson(), DateTime.UtcNow);
			jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, "Whatever", DateTime.Now, new Exception()));
			planningPeriod.JobResults.Add(jobResult);

			var result = (OkNegotiatedContentResult<List<PlanningPeriodModel>>)Target.GetAllPlanningPeriods(planningGroupId);
			result.Content.First().State.Should().Be(PlanningPeriodState.IntradayOptimizationFailed.ToString());
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
		
		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			//shouldn't be here
			//instead move logic to domain and don't fake other stuff than "out of process calls" (=repos and similar)
			//These out of process-fakes, put it in DomainTestAttribute
			extend.AddService<PlanningPeriodController>();
		}
	}
}
