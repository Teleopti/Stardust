using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationWeb
	{
		private readonly IDayOffOptimizationCommandHandler _dayOffOptimizationCommandHandler;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		[RemoveMeWithToggle(Toggles.ResourcePlanner_DayOffOptimizationIslands_47208)]
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly OptimizationResult _optimizationResult;
		private readonly IPersonRepository _personRepository;

		public DayOffOptimizationWeb(IDayOffOptimizationCommandHandler dayOffOptimizationCommandHandler,
			FillSchedulerStateHolder fillSchedulerStateHolder, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduleDictionaryPersister persister, 
			IPlanningPeriodRepository planningPeriodRepository,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			OptimizationResult optimizationResult,
			IPersonRepository personRepository)
		{
			_dayOffOptimizationCommandHandler = dayOffOptimizationCommandHandler;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
			_planningPeriodRepository = planningPeriodRepository;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_optimizationResult = optimizationResult;
			_personRepository = personRepository;
		}
		
		[TestLog]
		public virtual OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var optiData = Setup(planningPeriodId);
			_dayOffOptimizationCommandHandler.Execute(new DayOffOptimizationCommand
				{
					Period = optiData.Period,
					AgentsToOptimize = optiData.Agents,
					RunWeeklyRestSolver = true,
					PlanningPeriodId = planningPeriodId,
					FromPlan = true
				}, 
				null);
			optiData.PlanningPeriod.Scheduled();
			return _optimizationResult.Create(optiData.Period, optiData.Agents, 
				optiData.PlanningPeriod.PlanningGroup, _optimizationPreferencesProvider.Fetch().General.UsePreferences);
		}

		[UnitOfWork]
		protected virtual OptimizationData Setup(Guid planningPeriodId)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var planningGroup = planningPeriod.PlanningGroup;
			IEnumerable<IPerson> agents;
			if (planningGroup == null)
			{
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period);
				agents = schedulerStateHolder.SchedulingResultState.LoadedAgents.FixedStaffPeople(period);
			}
			else
			{
				var people = _personRepository.FindPeopleInPlanningGroup(planningGroup, period);
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period);
				agents = people.FixedStaffPeople(period);
			}
			return new OptimizationData
			{
				Period = period,
				Agents = agents,
				PlanningPeriod = planningPeriod
			};
		}
		
		protected class OptimizationData
		{
			public DateOnlyPeriod Period { get; set; }
			public IEnumerable<IPerson> Agents { get; set; }
			public IPlanningPeriod PlanningPeriod { get; set; }
		}




		[RemoveMeWithToggle(Toggles.ResourcePlanner_DayOffOptimizationIslands_47208)]
		public class DayOffOptimizationWebToggleOff : DayOffOptimizationWeb
		{
			public DayOffOptimizationWebToggleOff(IDayOffOptimizationCommandHandler dayOffOptimizationCommandHandler, FillSchedulerStateHolder fillSchedulerStateHolder, Func<ISchedulerStateHolder> schedulerStateHolder, IScheduleDictionaryPersister persister, IPlanningPeriodRepository planningPeriodRepository, IOptimizationPreferencesProvider optimizationPreferencesProvider, OptimizationResult optimizationResult, IPersonRepository personRepository) : base(dayOffOptimizationCommandHandler, fillSchedulerStateHolder, schedulerStateHolder, persister, planningPeriodRepository, optimizationPreferencesProvider, optimizationResult, personRepository)
			{
			}
			
			[TestLog]
			public override OptimizationResultModel Execute(Guid planningPeriodId)
			{
				var optiData = SetupAndRun(planningPeriodId);
				optiData.PlanningPeriod.Scheduled();
				_persister.Persist(_schedulerStateHolder().Schedules);
				return _optimizationResult.Create(optiData.Period, optiData.Agents, 
					optiData.PlanningPeriod.PlanningGroup, _optimizationPreferencesProvider.Fetch().General.UsePreferences);
			}

			[UnitOfWork]
			protected virtual OptimizationData SetupAndRun(Guid planningPeriodId)
			{
				var schedulerStateHolder = _schedulerStateHolder();
				var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
				var period = planningPeriod.Range;
				var planningGroup = planningPeriod.PlanningGroup;
				IEnumerable<IPerson> agents;
				if (planningGroup == null)
				{
					_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period);
					agents = schedulerStateHolder.SchedulingResultState.LoadedAgents.FixedStaffPeople(period);
				}
				else
				{
					var people = _personRepository.FindPeopleInPlanningGroup(planningGroup, period);
					_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period);
					agents = people.FixedStaffPeople(period);
				}
				
				_dayOffOptimizationCommandHandler.Execute(new DayOffOptimizationCommand
					{
						Period = period,
						AgentsToOptimize = agents,
						RunWeeklyRestSolver = true,
						PlanningPeriodId = planningPeriodId
					}, 
					null);
				return new OptimizationData
				{
					Period = period,
					Agents = agents,
					PlanningPeriod = planningPeriod
				};
			}
		}
	}
}