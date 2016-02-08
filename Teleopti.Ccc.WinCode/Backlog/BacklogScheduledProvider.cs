using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Backlog
{
	public class BacklogScheduledProvider
	{
		private DateOnlyPeriod _period;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private ISchedulerStateHolder _stateHolder;

		public BacklogScheduledProvider(IComponentContext componentContext, DateOnlyPeriod period)
		{
			_period = period;
			var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
			ILifetimeScope container = lifetimeScope.BeginLifetimeScope();
			_resourceOptimizationHelper = container.Resolve<IResourceOptimizationHelper>();
			_stateHolder = container.Resolve<ISchedulerStateHolder>();
		}

		public void Load()
		{
			var dateOnlyPeriodAsDateTimePeriod = new DateOnlyPeriodAsDateTimePeriod(_period, TimeZoneGuard.Instance.TimeZone);
			_stateHolder.RequestedPeriod = dateOnlyPeriodAsDateTimePeriod;

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var scenarioRepository = new ScenarioRepository(uow);
				loadCommonStateHolder(uow);
				loadSkills(uow, _stateHolder);
				_stateHolder.SetRequestedScenario(scenarioRepository.LoadDefaultScenario());
				//backgroundWorker.ReportProgress(0, "Loading forecasts...");
				loadSkillDays(uow, _stateHolder, dateOnlyPeriodAsDateTimePeriod.Period());
				//backgroundWorker.ReportProgress(0, "Loading schedules...");
				loadSchedules(uow, _stateHolder);
			}


			//backgroundWorker.ReportProgress(0, "Calculating resources...");
			foreach (var dateOnly in _period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, false);
			}

			//backgroundWorker.ReportProgress(0, "Initializing...");
		}

		public TimeSpan GetScheduledTimeOnDate(DateOnly date, ISkill skill)
		{
			var skillDay = _stateHolder.SchedulingResultState.SkillDayOnSkillAndDateOnly(skill, date);
			if (skillDay == null)
				return TimeSpan.Zero;

			return
				TimeSpan.FromHours(SkillStaffPeriodHelper.ScheduledHours(skillDay.SkillStaffPeriodCollection).GetValueOrDefault(0));
		}

		public TimeSpan GetForecastedTimeOnDate(DateOnly date, ISkill skill)
		{
			var skillDay = _stateHolder.SchedulingResultState.SkillDayOnSkillAndDateOnly(skill, date);
			if (skillDay == null)
				return TimeSpan.Zero;

			var ret = TimeSpan.Zero;
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				ret = ret.Add(skillStaffPeriod.ForecastedIncomingDemand());
			}
			return ret;
		}

		private void loadCommonStateHolder(IUnitOfWork uow)
		{
			_stateHolder.LoadCommonState(uow, new RepositoryFactory());
			if (!_stateHolder.CommonStateHolder.DayOffs.Any())
				throw new StateHolderException("You must create at least one Day Off in Options!");
		}

		private void loadSkills(IUnitOfWork uow, ISchedulerStateHolder stateHolder)
		{
			ICollection<ISkill> skills = new SkillRepository(uow).FindAllWithSkillDays(stateHolder.RequestedPeriod.DateOnlyPeriod);
			foreach (ISkill skill in skills)
			{
				if (skill.SkillType is SkillTypePhone)
					skill.SkillType.StaffingCalculatorService = new StaffingCalculatorServiceFacade();
				stateHolder.SchedulingResultState.AddSkills(skill);
			}
		}

		private void loadSkillDays(IUnitOfWork uow, ISchedulerStateHolder stateHolder, DateTimePeriod periodToLoad)
		{
			using (PerformanceOutput.ForOperation("Loading skill days"))
			{
				ISkillDayRepository skillDayRepository = new SkillDayRepository(uow);
				IMultisiteDayRepository multisiteDayRepository = new MultisiteDayRepository(uow);
				stateHolder.SchedulingResultState.SkillDays = new SkillDayLoadHelper(skillDayRepository, multisiteDayRepository).
					LoadSchedulerSkillDays(
						new DateOnlyPeriod(stateHolder.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-8),
							stateHolder.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(8)), stateHolder.SchedulingResultState.Skills,
						stateHolder.RequestedScenario);

				IList<ISkillStaffPeriod> skillStaffPeriods =
					stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
						stateHolder.SchedulingResultState.Skills, periodToLoad);

				foreach (ISkillStaffPeriod period in skillStaffPeriods)
				{
					period.Payload.UseShrinkage = false;
				}
			}
		}

		private void loadSchedules(IUnitOfWork uow, ISchedulerStateHolder stateHolder)
		{
			var period = new ScheduleDateTimePeriod(stateHolder.RequestedPeriod.Period(), stateHolder.SchedulingResultState.PersonsInOrganization);
			using (PerformanceOutput.ForOperation("Loading schedules " + period.LoadedPeriod()))
			{
				stateHolder.SchedulingResultState.PersonsInOrganization = new PersonRepository(new ThisUnitOfWork(uow)).FindPeopleInOrganization(_stateHolder.RequestedPeriod.DateOnlyPeriod, false);
				IPersonProvider personsInOrganizationProvider = new PersonsInOrganizationProvider(stateHolder.SchedulingResultState.PersonsInOrganization);
				// If the people in organization is filtered out to 70% or less of all people then flag 
				// so that a criteria for that is used later when loading schedules.
				//var loaderSpecification = new LoadScheduleByPersonSpecification();
				//personsInOrganizationProvider.DoLoadByPerson = loaderSpecification.IsSatisfiedBy(decider);
				IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false)
				{
					LoadDaysAfterLeft = true
				};
				stateHolder.LoadSchedules(new ScheduleStorage(uow), personsInOrganizationProvider, scheduleDictionaryLoadOptions, period);
			}
		}
	}
}