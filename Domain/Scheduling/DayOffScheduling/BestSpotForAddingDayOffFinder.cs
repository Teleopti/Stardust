using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IBestSpotForAddingDayOffFinder
	{
		DateOnly? Find(IList<IScheduleDayData> dataList);
	}

	public class BestSpotForAddingDayOffFinder : IBestSpotForAddingDayOffFinder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public DateOnly? Find(IList<IScheduleDayData> dataList)
		{
			for (int jumpIndex = 0; jumpIndex < 7; jumpIndex++)
			{
				for (int index = jumpIndex; index < dataList.Count; index++)
				{
					IScheduleDayData scheduleDayData = dataList[index];
					if (scheduleDayData.IsContractDayOff)
					{
						if (!scheduleDayData.IsScheduled && !scheduleDayData.IsDayOff && !scheduleDayData.HaveRestriction)
							return scheduleDayData.DateOnly;

						IScheduleDayData dayBefore = dataList[index - jumpIndex];
						if (dayBefore.IsContractDayOff || dayBefore.IsScheduled || dayBefore.HaveRestriction)
						{
							index += jumpIndex;
							continue;
						}

						return dataList[index - jumpIndex].DateOnly;
					}
				}
			}

			return null;
		}
	}
}