using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
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
		private readonly IIntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;

		public IntradayOptimizationFromWeb(IIntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
			IPlanningPeriodRepository planningPeriodRepository,
			IFixedStaffLoader fixedStaffLoader,
			IFillSchedulerStateHolder fillSchedulerStateHolder)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_planningPeriodRepository = planningPeriodRepository;
			_fixedStaffLoader = fixedStaffLoader;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
		}

		public virtual void Execute(Guid planningPeriodId)
		{
			var loadedData = LoadNecessaryData(planningPeriodId);
			_intradayOptimizationCommandHandler.Execute(new IntradayOptimizationCommand
			{
				Period = loadedData.Period,
				Agents = loadedData.Agents,
				RunResolveWeeklyRestRule = true
			});
		}

		[UnitOfWork]
		protected virtual WebIntradayCommandData LoadNecessaryData(Guid planningPeriodId)
		{
			var period = _planningPeriodRepository.Load(planningPeriodId).Range;
			return new WebIntradayCommandData
			{
				Period = period,
				Agents = _fixedStaffLoader.Load(period).AllPeople
			};
		}

		protected class WebIntradayCommandData
		{
			public DateOnlyPeriod Period { get; set; }
			public IEnumerable<IPerson> Agents { get; set; }
		}
	}
}