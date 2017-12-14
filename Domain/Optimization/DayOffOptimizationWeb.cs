using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
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
			var optiData = SetupAndOptimize(planningPeriodId);
			_persister.Persist(_schedulerStateHolder().Schedules);
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			return _optimizationResult.Create(optiData.DateOnlyPeriod, optiData.Persons, optiData.PlanningPeriod.PlanningGroup, optimizationPreferences.General.UsePreferences);
		}

		protected virtual OptimizationData SetupAndOptimize(Guid planningPeriodId)
		{
			var optData = Setup(planningPeriodId);
			
			_dayOffOptimizationCommandHandler.Execute(new DayOffOptimizationCommand
				{
					Period = optData.DateOnlyPeriod,
					AgentsToOptimize = optData.Persons,
					RunWeeklyRestSolver = true,
					PlanningPeriodId = planningPeriodId
				}, 
				null);

			optData.PlanningPeriod.Scheduled();

			return optData;
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
				agents = schedulerStateHolder.ChoosenAgents.FixedStaffPeople(period);
			}
			else
			{
				var people = _personRepository.FindPeopleInPlanningGroup(planningGroup, period);
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period);
				agents = people.FixedStaffPeople(period);
			}
			return new OptimizationData
			{
				DateOnlyPeriod = period,
				Persons = agents,
				PlanningPeriod = planningPeriod
			};
		}
		
		protected class OptimizationData
		{
			public DateOnlyPeriod DateOnlyPeriod { get; set; }
			public IEnumerable<IPerson> Persons { get; set; }
			public IPlanningPeriod PlanningPeriod { get; set; }
		}
	}
}