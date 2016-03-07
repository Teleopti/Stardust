using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationFromWeb
	{
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly OptimizationResult _optimizationResult;

		public IntradayOptimizationFromWeb(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
			IPlanningPeriodRepository planningPeriodRepository,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IFillSchedulerStateHolder fillSchedulerStateHolder,
			OptimizationResult optimizationResult)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_planningPeriodRepository = planningPeriodRepository;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_optimizationResult = optimizationResult;
		}

		public virtual OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var period = FillSchedulerStateHolder(planningPeriodId);
			_intradayOptimizationCommandHandler.Execute(new IntradayOptimizationCommand {Period = period, Agents = _schedulerStateHolder().AllPermittedPersons });
			return _optimizationResult.Create(period);
		}

		[UnitOfWork]
		protected virtual DateOnlyPeriod FillSchedulerStateHolder(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			_fillSchedulerStateHolder.Fill(_schedulerStateHolder(), period); //see if this can be made smarter - not ladda hela världen
			return period;
		}
	}
}