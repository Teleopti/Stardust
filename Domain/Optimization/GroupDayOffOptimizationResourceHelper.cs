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
			foreach (var scheduleDay in modifiedDays)
			{
				var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
				
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, false, false);
			}	
		}
	}
}
