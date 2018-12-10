using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IBestSpotForAddingDayOffFinder
	{
		DateOnly? Find(IList<IScheduleDayData> dataList);
	}

	public class BestSpotForAddingDayOffFinder : IBestSpotForAddingDayOffFinder
	{
		public DateOnly? Find(IList<IScheduleDayData> dataList)
		{
			DateOnly? oneDayThatMetCriteria = null;

			for (int jumpIndex = 0; jumpIndex < 7; jumpIndex++)
			{
				for (int index = jumpIndex; index < dataList.Count; index++)
				{
					IScheduleDayData scheduleDayData = dataList[index];
					if (!scheduleDayData.IsScheduled && !scheduleDayData.IsDayOff && !scheduleDayData.HaveRestriction)
						oneDayThatMetCriteria = scheduleDayData.DateOnly;

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

			return oneDayThatMetCriteria.HasValue ? oneDayThatMetCriteria : null;
		}
	}
}