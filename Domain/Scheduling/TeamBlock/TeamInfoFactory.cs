using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamInfoFactory
	{
		ITeamInfo CreateTeamInfo(IEnumerable<IPerson> personsInOrganisation, IPerson person, DateOnly dateOnly, IEnumerable<IScheduleMatrixPro> allMatrixesInScheduler);
		ITeamInfo CreateTeamInfo(IEnumerable<IPerson> personsInOrganisation, IPerson person, DateOnlyPeriod period, IEnumerable<IScheduleMatrixPro> allMatrixesInScheduler);
	}

	public class TeamInfoFactory : ITeamInfoFactory
	{
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;

		public TeamInfoFactory(IGroupPersonBuilderWrapper groupPersonBuilderWrapper)
		{
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
		}

		public ITeamInfo CreateTeamInfo(IEnumerable<IPerson> personsInOrganisation, IPerson person, DateOnly dateOnly, IEnumerable<IScheduleMatrixPro> allMatrixesInScheduler)
		{
		    if (allMatrixesInScheduler == null) throw new ArgumentNullException(nameof(allMatrixesInScheduler));
			DateOnly firstDateOfMatrix = dateOnly;
			Group group = _groupPersonBuilderWrapper.ForOptimization().BuildGroup(personsInOrganisation, person, firstDateOfMatrix);
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

		public ITeamInfo CreateTeamInfo(IEnumerable<IPerson> personsInOrganisation, IPerson person, DateOnlyPeriod period, IEnumerable<IScheduleMatrixPro> allMatrixesInScheduler)
		{
		    if (allMatrixesInScheduler == null) throw new ArgumentNullException(nameof(allMatrixesInScheduler));
		    Group group = _groupPersonBuilderWrapper.ForOptimization().BuildGroup(personsInOrganisation, person, period.StartDate);
		    if (group == null) return null;
			if (!group.GroupMembers.Any()) return null;

			IList<IList<IScheduleMatrixPro>> matrixesForGroup = new List<IList<IScheduleMatrixPro>>();
			foreach (var groupMember in group.GroupMembers)
			{
				IList<IScheduleMatrixPro> memberList =
					allMatrixesInScheduler.Where(matrixPro => matrixPro.Person.Equals(groupMember) && matrixPro.SchedulePeriod.DateOnlyPeriod.Intersection(period) != null).ToList();
				matrixesForGroup.Add(memberList);
			}

			return new TeamInfo(group, matrixesForGroup);
		}
	}
}