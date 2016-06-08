using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CalculateForReadModel
	{
		private readonly FillSchedulerStateHolderForResourceCalculation _fillSchedulerStateHolderForResourceCalculation;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public CalculateForReadModel(
			FillSchedulerStateHolderForResourceCalculation fillSchedulerStateHolderForResourceCalculation,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_fillSchedulerStateHolderForResourceCalculation = fillSchedulerStateHolderForResourceCalculation;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		[UnitOfWork, AsSystem]
		public virtual void ResourceCalculatePeriod(DateOnlyPeriod period)
		{
			_fillSchedulerStateHolderForResourceCalculation.Fill(_schedulerStateHolder(), null, null, null, period);
			foreach (var dateOnly in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly,true,true);
			}
			
		}
	}
}