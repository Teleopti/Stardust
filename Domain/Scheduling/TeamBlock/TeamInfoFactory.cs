using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamInfoFactory
	{
		ITeamInfo CreateTeamInfo(IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allMatrixesInScheduler);
		ITeamInfo CreateTeamInfo(IPerson person, DateOnlyPeriod period, IList<IScheduleMatrixPro> allMatrixesInScheduler);
	}

	public class TeamInfoFactory : ITeamInfoFactory
	{
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

		public TeamInfoFactory(IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
		}

		public ITeamInfo CreateTeamInfo(IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allMatrixesInScheduler)
		{
		    if (allMatrixesInScheduler == null) throw new ArgumentNullException("allMatrixesInScheduler");
			DateOnly firstDateOfMatrix = dateOnly;
			IGroupPerson groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(person, firstDateOfMatrix);
			IList<IList<IScheduleMatrixPro>> matrixesForGroup = new List<IList<IScheduleMatrixPro>>();
			foreach (var groupMember in groupPerson.GroupMembers)
			{
				foreach (var matrixPro in allMatrixesInScheduler)
				{
					if (matrixPro.Person.Equals(groupMember) && matrixPro.SchedulePeriod.DateOnlyPeriod.Contains(firstDateOfMatrix))
					{
						IList<IScheduleMatrixPro> memberList = new List<IScheduleMatrixPro>{matrixPro};
						matrixesForGroup.Add(memberList);
						break;
					}
				}
			}

			return new TeamInfo(groupPerson, matrixesForGroup);
		}

		public ITeamInfo CreateTeamInfo(IPerson person, DateOnlyPeriod period, IList<IScheduleMatrixPro> allMatrixesInScheduler)
		{
		    if (allMatrixesInScheduler == null) throw new ArgumentNullException("allMatrixesInScheduler");
		    IGroupPerson groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(person, period.StartDate);
		    if (groupPerson == null) return null;
			IList<IList<IScheduleMatrixPro>> matrixesForGroup = new List<IList<IScheduleMatrixPro>>();
			foreach (var groupMember in groupPerson.GroupMembers)
			{
				IList<IScheduleMatrixPro> memberList = new List<IScheduleMatrixPro>();
				foreach (var matrixPro in allMatrixesInScheduler)
				{
					if (matrixPro.Person.Equals(groupMember))
					{
						if(matrixPro.SchedulePeriod.DateOnlyPeriod.Intersection(period) != null)
							memberList.Add(matrixPro);	
					}
				}
				matrixesForGroup.Add(memberList);
			}

			return new TeamInfo(groupPerson, matrixesForGroup);
		}
	}
}