using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IUserTimeZone _userTimeZone;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillRepository _skillRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPeopleAndSkillLoaderDecider _decider;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly OutboundAssignedStaffProvider _outboundAssignedStaffProvider;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;

		public OutboundScheduledResourcesProvider(IResourceOptimizationHelper resourceOptimizationHelper,
			IUserTimeZone userTimeZone, Func<ISchedulerStateHolder> schedulerStateHolder, IScenarioRepository scenarioRepository,
			ISkillDayLoadHelper skillDayLoadHelper, ISkillRepository skillRepository, IScheduleRepository scheduleRepository,
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
			_scheduleRepository = scheduleRepository;
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
	        var campaignSkillsAndRelevantOtherSkills = new List<ISkill>();
	        foreach (var skill in allSkills)
	        {
		        if(skill.SkillType.ForecastSource == ForecastSource.OutboundTelephony)
					campaignSkillsAndRelevantOtherSkills.Add(skill);
	        }

			var deciderResult = _decider.Execute(scenario, dateTimePeriod, people.FixedStaffPeople);
			deciderResult.FilterPeople(people.AllPeople);

	        foreach (var skill in allSkills)
	        {
		        if(!campaignSkillsAndRelevantOtherSkills.Contains(skill))
					campaignSkillsAndRelevantOtherSkills.Add(skill);
	        }

			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, campaignSkillsAndRelevantOtherSkills, scenario);

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
			schedulerStateHolder.LoadSchedules(_scheduleRepository, new PersonsInOrganizationProvider(people.AllPeople),
				new ScheduleDictionaryLoadOptions(true, false, false),
				new ScheduleDateTimePeriod(dateTimePeriod, people.FixedStaffPeople, new SchedulerRangeToLoadCalculator(dateTimePeriod)));
	      
			foreach (var dateOnly in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, false);
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