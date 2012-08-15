using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupOptimizerValidateProposedDatesInSameMatrix
	{
		ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff);
	}

	public class GroupOptimizerValidateProposedDatesInSameMatrix : IGroupOptimizerValidateProposedDatesInSameMatrix
	{
		private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;

		public GroupOptimizerValidateProposedDatesInSameMatrix(IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup)
		{
			_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
		}

		public ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff)
		{
			ValidatorResult result = new ValidatorResult();
			
			List<IScheduleMatrixPro> all = new List<IScheduleMatrixPro>();
			foreach (var dateOnly in dates)
			{
				all.AddRange(_groupOptimizerFindMatrixesForGroup.Find(person, dateOnly));
			}

			HashSet<IScheduleMatrixPro> matrixList = new HashSet<IScheduleMatrixPro>(all);

			if (matrixList.Count == 0)
			{
				result = new ValidatorResult();
				return result;
			}

			foreach (var matrixPro in matrixList)
			{
				foreach (var dateOnly in dates)
				{
					if (!matrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
					{
						result.Success = true;
						result.MatrixList = new List<IScheduleMatrixPro>(matrixList);
						result.DaysToLock = new DateOnlyPeriod(dateOnly, dateOnly);
					}
				}
			}

			result.Success = true;
			return result;
		}
	}
}