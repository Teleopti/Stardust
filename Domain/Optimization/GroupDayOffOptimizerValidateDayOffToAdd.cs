using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupDayOffOptimizerValidateDayOffToAdd
	{
		ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff);
	}

	public class GroupDayOffOptimizerValidateDayOffToAdd : IGroupDayOffOptimizerValidateDayOffToAdd
	{
		private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;

		public GroupDayOffOptimizerValidateDayOffToAdd(IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup)
		{
			_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff)
		{
			ValidatorResult result = new ValidatorResult();

			if (useSameDaysOff)
			{
				foreach (var dateOnly in dates)
				{
					List<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>();
					matrixList.AddRange(_groupOptimizerFindMatrixesForGroup.Find(person, dateOnly));
					result.MatrixList = matrixList;
					foreach (var matrixPro in matrixList)
					{
						IScheduleDayPro scheduleDayPro = matrixPro.GetScheduleDayByKey(dateOnly);
						if (!matrixPro.UnlockedDays.Contains(scheduleDayPro))
						{
							result.Success = true;
							result.DaysToLock = new DateOnlyPeriod(dateOnly, dateOnly);
							return result;
						}

						SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
						if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
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