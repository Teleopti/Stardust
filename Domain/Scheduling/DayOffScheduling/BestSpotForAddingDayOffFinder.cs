using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IBestSpotForAddingDayOffFinder
	{
		DateOnly? Find(IDictionary<DateOnly, IScheduleDayData> dataList);
	}

	public class BestSpotForAddingDayOffFinder : IBestSpotForAddingDayOffFinder
	{
		public DateOnly? Find(IDictionary<DateOnly, IScheduleDayData> dataList)
		{
			IList<IScheduleDayData> list = new List<IScheduleDayData>(dataList.Values);
			for (int jumpIndex = 1; jumpIndex < 7; jumpIndex++)
			{
				for (int index = jumpIndex; index < list.Count; index++)
				{
					IScheduleDayData scheduleDayData = list[index];
					if (scheduleDayData.IsContractDayOff)
					{
						IScheduleDayData dayBefore = list[index - jumpIndex];
						if (dayBefore.IsContractDayOff || dayBefore.IsScheduled || dayBefore.HaveRestriction)
						{
							index += jumpIndex;
							continue;
						}

						return list[index - jumpIndex].DateOnly;
					}
				}
			}

			return null;
		}
	}
}