using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupMoveTimeOptimizationResourceHelper
	{
		void CalculateDeletedDays(IList<IScheduleDay> daysToDelete);
		void Rollback(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IScheduleDictionary scheduleDictionary);
	}

	public class GroupMoveTimeOptimizationResourceHelper : IGroupMoveTimeOptimizationResourceHelper
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public GroupMoveTimeOptimizationResourceHelper(IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void CalculateDeletedDays(IList<IScheduleDay> daysToDelete)	
		{
			var dates = new List<DateOnly>();
			foreach (var scheduleDay in daysToDelete)
			{
				if (!dates.Contains(scheduleDay.DateOnlyAsPeriod.DateOnly))
					dates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
			}

			foreach (var dateOnly in dates)
			{
                if (daysToDelete.Any(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly == dateOnly))
				{
					_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
				}
			}		
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Rollback(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IScheduleDictionary scheduleDictionary)
		{
			var dates = new List<DateOnly>();

			foreach (var modifiedSchedule in schedulePartModifyAndRollbackService.ModificationCollection.ToList())
			{
				var modifiedDate = modifiedSchedule.DateOnlyAsPeriod.DateOnly;

				if (!dates.Contains(modifiedDate))
					dates.Add(modifiedDate);
			}

			schedulePartModifyAndRollbackService.Rollback();

			foreach (var date in dates)
			{
				_resourceOptimizationHelper.ResourceCalculateDate(date, true, true);
                if (!dates.Contains(date.AddDays(1)))
                {
                    _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true);
                }
			}
		}
	}
}
