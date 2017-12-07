using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationWeb
	{
		private readonly DayOffOptimization _dayOffOptimization;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly OptimizationResult _optimizationResult;
		private readonly IPersonRepository _personRepository;
		private readonly BlockPreferenceProviderUsingFiltersFactory _blockPreferenceProviderUsingFiltersFactory;

		public DayOffOptimizationWeb(DayOffOptimization dayOffOptimization,
			FillSchedulerStateHolder fillSchedulerStateHolder, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduleDictionaryPersister persister, 
			IPlanningPeriodRepository planningPeriodRepository,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
			OptimizationResult optimizationResult,
			IPersonRepository personRepository,
			BlockPreferenceProviderUsingFiltersFactory blockPreferenceProviderUsingFiltersFactory)
		{
			_dayOffOptimization = dayOffOptimization;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
			_planningPeriodRepository = planningPeriodRepository;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_optimizationResult = optimizationResult;
			_personRepository = personRepository;
			_blockPreferenceProviderUsingFiltersFactory = blockPreferenceProviderUsingFiltersFactory;
		}

		public virtual OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var optiData = SetupAndOptimize(planningPeriodId);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return _optimizationResult.Create(optiData.DateOnlyPeriod, optiData.Persons, optiData.PlanningGroup, optiData.UsePreferences);
		}

		[UnitOfWork]
		[TestLog]
		protected virtual OptimizationData SetupAndOptimize(Guid planningPeriodId)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider;
			IBlockPreferenceProvider blockPreferenceProvider;
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var planningGroup = planningPeriod.PlanningGroup;
			IEnumerable<IPerson> agents;
			if (planningGroup == null)
			{
				dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
				blockPreferenceProvider = _blockPreferenceProviderUsingFiltersFactory.Create();
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, null, period);
				agents = schedulerStateHolder.ChoosenAgents.FixedStaffPeople(period);
			}
			else
			{
				dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create(planningGroup);
				blockPreferenceProvider = _blockPreferenceProviderUsingFiltersFactory.Create(planningGroup);
				var people = _personRepository.FindPeopleInPlanningGroup(planningGroup, period);
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, null, period);
				agents = people.FixedStaffPeople(period);
			}
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);

			_dayOffOptimization.Execute(period,
				agents.ToList(),
				optimizationPreferences,
				schedulingOptions,
				dayOffOptimizationPreferenceProvider,
				blockPreferenceProvider,
				new NoSchedulingProgress(),
				true,
				null);

			planningPeriod.Scheduled();

			return new OptimizationData
			{
				DateOnlyPeriod = period,
				Persons = agents,
				PlanningGroup = planningGroup,
				UsePreferences = schedulingOptions.UsePreferences
			};
		}
	}
}