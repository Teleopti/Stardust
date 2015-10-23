using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
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
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;

		public TeamInfoFactory(IGroupPersonBuilderWrapper groupPersonBuilderWrapper)
		{
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
		}

		public ITeamInfo CreateTeamInfo(IPerson person, DateOnly dateOnly, IList<IScheduleMatrixPro> allMatrixesInScheduler)
		{
		    if (allMatrixesInScheduler == null) throw new ArgumentNullException("allMatrixesInScheduler");
			DateOnly firstDateOfMatrix = dateOnly;
			Group group = _groupPersonBuilderWrapper.ForOptimization().BuildGroup(person, firstDateOfMatrix);
			if (!group.GroupMembers.Any()) return null;

			IList<IList<IScheduleMatrixPro>> matrixesForGroup = new List<IList<IScheduleMatrixPro>>();
			foreach (var groupMember in group.GroupMembers)
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

			return new TeamInfo(group, matrixesForGroup);
		}

		public ITeamInfo CreateTeamInfo(IPerson person, DateOnlyPeriod period, IList<IScheduleMatrixPro> allMatrixesInScheduler)
		{
		    if (allMatrixesInScheduler == null) throw new ArgumentNullException("allMatrixesInScheduler");
		    Group group = _groupPersonBuilderWrapper.ForOptimization().BuildGroup(person, period.StartDate);
		    if (group == null) return null;
			if (!group.GroupMembers.Any()) return null;

			IList<IList<IScheduleMatrixPro>> matrixesForGroup = new List<IList<IScheduleMatrixPro>>();
			foreach (var groupMember in group.GroupMembers)
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

			return new TeamInfo(group, matrixesForGroup);
		}
	}
}