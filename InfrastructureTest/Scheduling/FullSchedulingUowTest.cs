﻿using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling
{
	[DatabaseTest]
	public class FullSchedulingUowTest
	{
		public FullScheduling Target;
		public IScenarioRepository ScenarioRepository;
		public IDayOffTemplateRepository DayOffTemplateRepository;
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IRuleSetBagRepository RuleSetBagRepository;
		public IWorkShiftRuleSetRepository WorkShiftRuleSetRepository;
		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IPlanningPeriodRepository PlanningPeriodRepository;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public IJobResultRepository JobResultRepository;

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldDoSchedulingForPlanningPeriod(bool teamScheduling)
		{
			if (teamScheduling)
			{
				var defaultOptions = SchedulingOptionsProvider.Fetch(new DayOffTemplate());
				defaultOptions.UseTeam = true;
				defaultOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag);
				SchedulingOptionsProvider.SetFromTest(defaultOptions);
			}

			var planningPeriod = fillDatabaseWithEnoughDataToRunScheduling();

			Target.DoScheduling(planningPeriod.Id.Value);
		}

		private PlanningPeriod fillDatabaseWithEnoughDataToRunScheduling()
		{
			var scenario = new Scenario("_") { DefaultScenario = true };
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(1, 1, 1, 1, 1), new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCategory)){Description = new Description("_")};
			var ruleSetBag = new RuleSetBag(ruleSet) {Description = new Description("_")};
			var team = new Team();
			team.SetDescription(new Description("_"));
			team.Site = new Site("_");
			var date = new DateOnly(2017, 6, 1);
			var agent = new Person()
				.WithPersonPeriod(ruleSetBag, null, team).InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneWeek(date);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var planningPeriod = new PlanningPeriod(period);

			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")));
				SiteRepository.Add(agent.Period(date).Team.Site);
				TeamRepository.Add(agent.Period(date).Team);
				PartTimePercentageRepository.Add(agent.Period(date).PersonContract.PartTimePercentage);
				ContractRepository.Add(agent.Period(date).PersonContract.Contract);
				ContractScheduleRepository.Add(agent.Period(date).PersonContract.ContractSchedule);
				ActivityRepository.Add(activity);
				ShiftCategoryRepository.Add(shiftCategory);
				WorkShiftRuleSetRepository.Add(agent.Period(date).RuleSetBag.RuleSetCollection.Single());
				RuleSetBagRepository.Add(agent.Period(date).RuleSetBag);
				PersonRepository.Add(agent);
				var jobResult = new JobResult(JobCategory.WebSchedule, period, agent, DateTime.UtcNow);
				JobResultRepository.Add(jobResult);
				PlanningPeriodRepository.Add(planningPeriod);
				uow.PersistAll();
			}
			return planningPeriod;
		}
	}
}