using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupOptimizerValidateProposedDatesInSameMatrix
	{
		ValidatorResult Validate(IScheduleMatrixPro matrix, IList<DateOnly> dates, ISchedulingOptions schedulingOptions);
	}

	public class GroupOptimizerValidateProposedDatesInSameMatrix : IGroupOptimizerValidateProposedDatesInSameMatrix
	{
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private readonly IList<IScheduleMatrixPro> _allMatrixes;

		public GroupOptimizerValidateProposedDatesInSameMatrix(IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, 
			IList<IScheduleMatrixPro> allMatrixes)
		{
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
			_allMatrixes = allMatrixes;
		}

		public ValidatorResult Validate(IScheduleMatrixPro matrix, IList<DateOnly> dates, ISchedulingOptions schedulingOptions)
		{
			ValidatorResult result = new ValidatorResult();
			HashSet<IScheduleMatrixPro> matrixList = new HashSet<IScheduleMatrixPro>();

			foreach (var dateOnly in dates)
			{
				IGroupPerson groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(matrix.Person, dateOnly);
				if (groupPerson == null)
				{
					//report that the person does not belong to any group and return false when broken out
					result = new ValidatorResult();
					return result;
				}

				foreach (var person in groupPerson.GroupMembers)
				{
					foreach (var matrixPro in _allMatrixes)
					{
						if (matrixPro.Person.Equals(person))
						{
							if (matrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
							{
								matrixList.Add(matrixPro);
							}
						}
					}
				}
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