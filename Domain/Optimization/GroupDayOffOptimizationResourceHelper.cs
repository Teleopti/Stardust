using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupDayOffOptimizationResourceHelper
	{
		void ResourceCalculateContainersToRemove(IList<IScheduleDay> orgDays, IList<IScheduleDay> modifiedDays);
	}

	public class GroupDayOffOptimizationResourceHelper : IGroupDayOffOptimizationResourceHelper
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public GroupDayOffOptimizationResourceHelper(IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void ResourceCalculateContainersToRemove(IList<IScheduleDay> orgDays, IList<IScheduleDay> modifiedDays)	
		{
			var toRemove = new List<IScheduleDay>();
			var toAdd = new List<IScheduleDay>();

			foreach (var scheduleDay in modifiedDays)
			{
				var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
				var modifiedSignificantPart = scheduleDay.SignificantPart();

				if (modifiedSignificantPart == SchedulePartView.DayOff)
				{
					foreach (var orgDay in orgDays)
					{
						if (orgDay.DateOnlyAsPeriod.DateOnly == dateOnly && orgDay.SignificantPart() == SchedulePartView.MainShift)
						{
							toRemove.Add(orgDay);
						}
					}
				}

				else if (modifiedSignificantPart == SchedulePartView.MainShift)
				{
					foreach (var orgDay in orgDays)
					{
						if (orgDay.DateOnlyAsPeriod.DateOnly == dateOnly && orgDay.SignificantPart() == SchedulePartView.MainShift)
						{
							toRemove.Add(orgDay);
						}
					}

					toAdd.Add(scheduleDay);
				}

				if (toRemove.Count > 0 || toAdd.Count > 0)
					_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, false, false, toRemove, toAdd);
			}	
		}
	}
}
