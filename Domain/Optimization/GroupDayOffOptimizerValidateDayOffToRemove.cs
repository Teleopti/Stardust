using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupDayOffOptimizerValidateDayOffToRemove
	{
		bool Validate(IScheduleMatrixPro matrix, IList<DateOnly> dates, ISchedulingOptions schedulingOptions);
	}

	public class GroupDayOffOptimizerValidateDayOffToRemove : IGroupDayOffOptimizerValidateDayOffToRemove
	{
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private readonly IList<IScheduleMatrixPro> _allMatrixes;

		public GroupDayOffOptimizerValidateDayOffToRemove(IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, 
			IList<IScheduleMatrixPro> allMatrixes)
		{
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
			_allMatrixes = allMatrixes;
		}

		public bool Validate(IScheduleMatrixPro matrix, IList<DateOnly> dates, ISchedulingOptions schedulingOptions)
		{
			foreach (var dateOnly in dates)
			{
				IGroupPerson groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(matrix.Person, dateOnly);
				if (groupPerson == null)
				{
					//report that the person does not belong to any group and return false when broken out
					return false;
				}

				IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>();
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

				if (schedulingOptions.UseSameDayOffs)
				{
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
					}

					if (fail)
					{
						foreach (var matrixPro in matrixList)
						{
							matrixPro.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
						}

						return true;
					}
				}
			}
			return true;
		}
	}
}