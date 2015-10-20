

using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISafeRollbackAndResourceCalculation
	{
		void Execute(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ISchedulingOptions schedulingOptions);
	}

	public class SafeRollbackAndResourceCalculation : ISafeRollbackAndResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public SafeRollbackAndResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ISchedulingOptions schedulingOptions)
		{
			var modifyedScheduleDays = schedulePartModifyAndRollbackService.ModificationCollection.ToList();
			schedulePartModifyAndRollbackService.Rollback();
			HashSet<DateOnly> dates = new HashSet<DateOnly>();

			foreach (var modifyedScheduleDay in modifyedScheduleDays)
			{
				dates.Add(modifyedScheduleDay.DateOnlyAsPeriod.DateOnly);
			}

			IList<DateOnly> initialDates = new List<DateOnly>(dates);
			foreach (var initialDate in initialDates)
			{
				dates.Add(initialDate.AddDays(1));
			}

			foreach (var dateOnly in dates)
			{
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, schedulingOptions.ConsiderShortBreaks);
			}
		}
	}
}