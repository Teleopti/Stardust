﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
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
		private readonly MatrixListFactory _matrixListFactory;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly OptimizationResult _optimizationResult;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPersonRepository _personRepository;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly BlockPreferenceProviderUsingFiltersFactory _blockPreferenceProviderUsingFiltersFactory;

		public DayOffOptimizationWeb(DayOffOptimization dayOffOptimization,
			FillSchedulerStateHolder fillSchedulerStateHolder, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduleDictionaryPersister persister, 
			IPlanningPeriodRepository planningPeriodRepository,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			MatrixListFactory matrixListFactory,
			DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			OptimizationResult optimizationResult,
			IResourceCalculation resourceOptimizationHelper,
			IUserTimeZone userTimeZone,
			IPersonRepository personRepository,
			IResourceCalculation resourceCalculation,
			TeamInfoFactoryFactory teamInfoFactoryFactory, 
			BlockPreferenceProviderUsingFiltersFactory blockPreferenceProviderUsingFiltersFactory)
		{
			_dayOffOptimization = dayOffOptimization;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
			_planningPeriodRepository = planningPeriodRepository;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_matrixListFactory = matrixListFactory;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_optimizationResult = optimizationResult;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_userTimeZone = userTimeZone;
			_personRepository = personRepository;
			_resourceCalculation = resourceCalculation;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
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

			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, agents, period);
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences); 
			var resourceCalcDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);

			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.SchedulingResultState, true, period.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(period.Inflate(1), new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, false, false));
				_dayOffOptimization.Execute(matrixListForDayOffOptimization, period,
					agents.ToList(),
					optimizationPreferences,
					schedulingOptions,
					resourceCalcDelayer, 
					dayOffOptimizationPreferenceProvider,
					blockPreferenceProvider,
					_teamInfoFactoryFactory.Create(agents, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer),
					new NoSchedulingProgress(),
					true);
			}

			planningPeriod.Scheduled();

			return new  OptimizationData
			{
				DateOnlyPeriod = period,
				Persons = agents,
				PlanningGroup = planningGroup,
				UsePreferences = schedulingOptions.UsePreferences
			};
		}
	}
}