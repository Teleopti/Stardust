using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationFromWeb
	{
		private readonly IIntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public IntradayOptimizationFromWeb(IIntradayOptimizationCommandHandler intradayOptimizationCommandHandler, IPlanningPeriodRepository planningPeriodRepository)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_planningPeriodRepository = planningPeriodRepository;
		}

		public virtual void Execute(Guid planningPeriodId)
		{
			var period = LoadNecessaryData(planningPeriodId);
			_intradayOptimizationCommandHandler.Execute(new IntradayOptimizationCommand
			{
				Period = period,
				RunResolveWeeklyRestRule = true
			});
		}

		[UnitOfWork]
		protected virtual DateOnlyPeriod LoadNecessaryData(Guid planningPeriodId)
		{
			return _planningPeriodRepository.Load(planningPeriodId).Range;
		}
	}
}