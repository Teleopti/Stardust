using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling
{
	[DatabaseTest]
	[ExtendScope(typeof(WebIntradayOptimizationStardustHandler))]
	public class IntradayOptimizationStardustTest
	{
		public IntradayOptimizationFromWeb Target;

		public IActivityRepository ActivityRepository;
		public IScenarioRepository ScenarioRepository;
		public IPersonRepository PersonRepository;
		public ISkillRepository SkillRepository;
		public IWorkloadRepository WorkloadRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IPlanningPeriodRepository PlanningPeriodRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IJobResultRepository JobResultRepository;
		public IPlanningGroupSettingsRepository PlanningGroupSettingsRepository;
		public IPlanningGroupRepository PlanningGroupRepository;
		
		
		[Test]
		public void ShouldNotCrashReadingBlockPreference()
		{
			var date = new DateOnly(2000, 1, 1);
			var scenario = new Scenario("_") { DefaultScenario = true };
			var planningPeriod = new PlanningPeriod(date.ToDateOnlyPeriod(), SchedulePeriodType.Day, 1);
			var activity = new Activity("_");
			var skill = new Skill().IsOpen().InTimeZone(TimeZoneInfo.Utc);
			skill.Activity = activity;
			skill.SkillType.Description = new Description("_");
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill);
			var personPeriod = agent.Period(date);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				SiteRepository.Add(personPeriod.Team.Site);
				TeamRepository.Add(personPeriod.Team);
				PartTimePercentageRepository.Add(personPeriod.PersonContract.PartTimePercentage);
				ContractRepository.Add(personPeriod.PersonContract.Contract);
				ContractScheduleRepository.Add(personPeriod.PersonContract.ContractSchedule);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				PersonRepository.Add(agent);
				PlanningPeriodRepository.Add(planningPeriod);
				
				WorkloadRepository.AddRange(skill.WorkloadCollection);
				uow.PersistAll();
			}
			
			Target.Execute(planningPeriod.Id.Value, true);

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				JobResultRepository
					.LoadAllWithNoLock()
					.Single()
					.RethrowIfException();
			}
		}

		[Test]
		[Ignore("48512 - to be fixed")]
		public void ShouldNotCrashReadingPlanningGroupSettingFilters()
		{
			var date = new DateOnly(2000, 1, 1);
			var scenario = new Scenario("_") { DefaultScenario = true };
			var activity = new Activity("_");
			var skill = new Skill().IsOpen().InTimeZone(TimeZoneInfo.Utc);
			skill.Activity = activity;
			skill.SkillType.Description = new Description("_");
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill);
			var personPeriod = agent.Period(date);
			var planningGroup = new PlanningGroup("_");
			var planningGroupSetting = new PlanningGroupSettings(planningGroup);
			var planningPeriod = new PlanningPeriod(date.ToDateOnlyPeriod(), SchedulePeriodType.Day, 1, planningGroup);

			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				SiteRepository.Add(personPeriod.Team.Site);
				TeamRepository.Add(personPeriod.Team);
				PartTimePercentageRepository.Add(personPeriod.PersonContract.PartTimePercentage);
				ContractRepository.Add(personPeriod.PersonContract.Contract);
				ContractScheduleRepository.Add(personPeriod.PersonContract.ContractSchedule);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				PersonRepository.Add(agent);
				PlanningGroupRepository.Add(planningGroup);
				PlanningGroupSettingsRepository.Add(planningGroupSetting);
				PlanningPeriodRepository.Add(planningPeriod);

				WorkloadRepository.AddRange(skill.WorkloadCollection);
				uow.PersistAll();
			}

			Target.Execute(planningPeriod.Id.Value, true);

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				JobResultRepository
					.LoadAllWithNoLock()
					.Single()
					.RethrowIfException();
			}
		}
	}
}