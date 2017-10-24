﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public interface IOutboundScheduledResourcesProvider
	{
		void Load(IList<IOutboundCampaign> campaigns, DateOnlyPeriod period);
		TimeSpan GetScheduledTimeOnDate(DateOnly date, ISkill skill);
		TimeSpan GetForecastedTimeOnDate(DateOnly date, ISkill skill);
		void SetForecastedTimeOnDate(DateOnly date, ISkill skill, TimeSpan time);
	}

	public class OutboundScheduledResourcesProvider : IOutboundScheduledResourcesProvider
	{
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IUserTimeZone _userTimeZone;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillRepository _skillRepository;
		private readonly ScheduleStorage _scheduleStorage;
		private readonly IPeopleAndSkillLoaderDecider _decider;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly OutboundAssignedStaffProvider _outboundAssignedStaffProvider;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;

		public OutboundScheduledResourcesProvider(IResourceCalculation resourceOptimizationHelper,
			IUserTimeZone userTimeZone, Func<ISchedulerStateHolder> schedulerStateHolder, IScenarioRepository scenarioRepository,
			ISkillDayLoadHelper skillDayLoadHelper, ISkillRepository skillRepository, ScheduleStorage scheduleStorage,
			IPeopleAndSkillLoaderDecider decider,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IRepositoryFactory repositoryFactory,
			OutboundAssignedStaffProvider outboundAssignedStaffProvider,
			Func<IPersonSkillProvider> personSkillProvider)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_userTimeZone = userTimeZone;
			_schedulerStateHolder = schedulerStateHolder;
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillRepository = skillRepository;
			_scheduleStorage = scheduleStorage;
			_decider = decider;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
			_outboundAssignedStaffProvider = outboundAssignedStaffProvider;
			_personSkillProvider = personSkillProvider;
		}

        public void Load(IList<IOutboundCampaign> campaigns, DateOnlyPeriod period)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var timeZone = _userTimeZone.TimeZone();

			var allSkills = _skillRepository.FindAllWithSkillDays(period).ToArray();
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);
			var people = _outboundAssignedStaffProvider.Load(campaigns, period);
			initializePersonSkillProviderBeforeAccessingItFromOtherThreads(period, people.AllPeople);       
			var deciderResult = _decider.Execute(scenario, dateTimePeriod, people.FixedStaffPeople);
			deciderResult.FilterPeople(people.AllPeople);
			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, allSkills, scenario);
			var schedulerStateHolder = _schedulerStateHolder();
			var stateHolder = schedulerStateHolder.SchedulingResultState;
			stateHolder.PersonsInOrganization = people.AllPeople;
			stateHolder.SkillDays = forecast;
			stateHolder.AddSkills(allSkills);
			deciderResult.FilterSkills(allSkills,stateHolder.RemoveSkill,s => stateHolder.AddSkills(s));
			
			schedulerStateHolder.SetRequestedScenario(scenario);
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
			people.AllPeople.ForEach(schedulerStateHolder.AllPermittedPersons.Add);
			schedulerStateHolder.LoadCommonState(_currentUnitOfWorkFactory.Current().CurrentUnitOfWork(),
				_repositoryFactory);
			schedulerStateHolder.ResetFilteredPersons();
			//TODO: using schedulestorage here will load unnecesary big period
			schedulerStateHolder.LoadSchedules(_scheduleStorage, new PersonProvider(people.AllPeople),
				new ScheduleDictionaryLoadOptions(true, false, false),
				new ScheduleDateTimePeriod(dateTimePeriod, people.FixedStaffPeople, new SchedulerRangeToLoadCalculator(dateTimePeriod)));

			var resCalcData = _schedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(true, false);
			foreach (var dateOnly in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
			}
		}

		public TimeSpan GetScheduledTimeOnDate(DateOnly date, ISkill skill)
		{
			var skillDay = _schedulerStateHolder().SchedulingResultState.SkillDayOnSkillAndDateOnly(skill, date);
			if (skillDay == null)
				return TimeSpan.Zero;

			return
				TimeSpan.FromHours(SkillStaffPeriodHelper.ScheduledHours(skillDay.SkillStaffPeriodCollection).GetValueOrDefault(0));
		}

		public void SetForecastedTimeOnDate(DateOnly date, ISkill skill, TimeSpan time)
		{
			var skillDay = _schedulerStateHolder().SchedulingResultState.SkillDayOnSkillAndDateOnly(skill, date);
			if (skillDay == null)
				return;

			var periodCount = skillDay.SkillStaffPeriodCollection.Count;
			var timeOnEachPeriod = TimeSpan.FromTicks(time.Ticks/periodCount);
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				var newDemand = timeOnEachPeriod.TotalMinutes/skillStaffPeriod.Period.ElapsedTime().TotalMinutes;
				((SkillStaff) skillStaffPeriod.Payload).ForecastedIncomingDemand = newDemand;
			}
		}

		public TimeSpan GetForecastedTimeOnDate(DateOnly date, ISkill skill)
		{
			var skillDay = _schedulerStateHolder().SchedulingResultState.SkillDayOnSkillAndDateOnly(skill, date);
			if (skillDay == null)
				return TimeSpan.Zero;

			var ret = TimeSpan.Zero;
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				ret = ret.Add(skillStaffPeriod.ForecastedIncomingDemand());
			}
			return ret;
		}

		private void initializePersonSkillProviderBeforeAccessingItFromOtherThreads(DateOnlyPeriod period,
			IEnumerable<IPerson> allPeople)
		{
			var provider = _personSkillProvider();
			var dayCollection = period.DayCollection();
			allPeople.ForEach(p => dayCollection.ForEach(d => provider.SkillsOnPersonDate(p, d)));
		}
	}
}