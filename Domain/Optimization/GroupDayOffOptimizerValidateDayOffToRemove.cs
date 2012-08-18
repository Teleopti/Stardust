using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupDayOffOptimizerValidateDayOffToRemove
	{
		ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff);
	}

	public class GroupDayOffOptimizerValidateDayOffToRemove : IGroupDayOffOptimizerValidateDayOffToRemove
	{
		private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;

		public GroupDayOffOptimizerValidateDayOffToRemove(IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup)
		{
			_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
		}

		public ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff)
		{
			ValidatorResult result = new ValidatorResult();

			if(useSameDaysOff)
			{
				foreach (var dateOnly in dates)
				{
					List<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>();
					matrixList.AddRange(_groupOptimizerFindMatrixesForGroup.Find(person, dateOnly));


					bool fail = false;
					foreach (var matrixPro in matrixList)
					{
						IScheduleDayPro scheduleDayPro = matrixPro.GetScheduleDayByKey(dateOnly);
						if (!matrixPro.UnlockedDays.Contains(scheduleDayPro))
						{
							fail = true;
						}

						if (scheduleDayPro.DaySchedulePart().SignificantPart() != SchedulePartView.DayOff &&
							scheduleDayPro.DaySchedulePart().SignificantPart() != SchedulePartView.ContractDayOff)
						{
							fail = true;
						}

						if (fail)
						{
							result.Success = true;
							result.DaysToLock = new DateOnlyPeriod(dateOnly, dateOnly);
							return result;
						}
					}
				}
			}
			
			result.Success = true;
			return result;
		}
	}
}