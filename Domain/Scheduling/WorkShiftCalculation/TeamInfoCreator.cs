

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ITeamInfoCreator
	{
		TeamInfo CreateTeamInfo(IPerson person, DateOnly date, IList<IScheduleMatrixPro> allMatrixesInScheduler);
	}

	public class TeamInfoCreator : ITeamInfoCreator
	{
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

		public TeamInfoCreator(IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
		}

		public TeamInfo CreateTeamInfo(IPerson person, DateOnly date, IList<IScheduleMatrixPro> allMatrixesInScheduler)
		{
			DateOnly firstDateOfMatrix = date;
			IGroupPerson groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(person, firstDateOfMatrix);
			IList<IScheduleMatrixPro> matrixesForGroup = new List<IScheduleMatrixPro>();
			foreach (var groupMember in groupPerson.GroupMembers)
			{
				foreach (var matrixPro in allMatrixesInScheduler)
				{
					if (matrixPro.Person.Equals(groupMember) && matrixPro.SchedulePeriod.DateOnlyPeriod.Contains(firstDateOfMatrix))
					{
						matrixesForGroup.Add(matrixPro);
						break;
					}
				}
			}

			return new TeamInfo(groupPerson, matrixesForGroup);
		}
	}
}