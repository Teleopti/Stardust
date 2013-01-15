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
				var toDelete = new List<IScheduleDay>();

				foreach (var scheduleDay in daysToDelete)
				{
					if (scheduleDay.DateOnlyAsPeriod.DateOnly == dateOnly)
					{
						toDelete.Add(scheduleDay);
					}
				}

				if (toDelete.Count > 0)
				{
					_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true, toDelete, new List<IScheduleDay>());
				}
			}		
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Rollback(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IScheduleDictionary scheduleDictionary)
		{
			var dates = new List<DateOnly>();
			var modifiedDays = new List<IScheduleDay>();
			var orgDays = new List<IScheduleDay>();

			foreach (var modifiedSchedule in schedulePartModifyAndRollbackService.ModificationCollection.ToList())
			{
				var modifiedDate = modifiedSchedule.DateOnlyAsPeriod.DateOnly;

				if (!dates.Contains(modifiedDate))
					dates.Add(modifiedDate);

				modifiedDays.Add(modifiedSchedule);

				var scheduleRange = scheduleDictionary[modifiedSchedule.Person];
				var orgDay = scheduleRange.ScheduledDay(modifiedDate);
				orgDays.Add(orgDay);
			}

			schedulePartModifyAndRollbackService.Rollback();

			foreach (var date in dates)
			{
				var toRemove = new List<IScheduleDay>();
				var toAdd = new List<IScheduleDay>();

				var toRemoveNextDay = new List<IScheduleDay>();
				var toAddNextDay = new List<IScheduleDay>();

				foreach (var modifiedDay in modifiedDays)
				{
					if (modifiedDay.DateOnlyAsPeriod.DateOnly == date && modifiedDay.HasProjection)
					{
						toAdd.Add(modifiedDay);

						var modifiedPeriod = modifiedDay.Period;

						if (modifiedPeriod.StartDateTime.Date != modifiedPeriod.EndDateTime.Date)
							toAddNextDay.Add(modifiedDay);
					}
				}

				foreach (var orgDay in orgDays)
				{
					if (orgDay.DateOnlyAsPeriod.DateOnly == date && orgDay.HasProjection)
					{
						toRemove.Add(orgDay);

						var orgDayPeriod = orgDay.Period;

						if (orgDayPeriod.StartDateTime.Date != orgDayPeriod.EndDateTime.Date)
							toRemoveNextDay.Add(orgDay);
					}
				}

				if (toRemove.Count > 0 || toAdd.Count > 0)
				{
					_resourceOptimizationHelper.ResourceCalculateDate(date, true, true, toRemove, toAdd);
				}

				if (toRemoveNextDay.Count > 0 || toAddNextDay.Count > 0)
				{
					_resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true, toRemoveNextDay, toAddNextDay);
				}
			}
		}
	}
}
