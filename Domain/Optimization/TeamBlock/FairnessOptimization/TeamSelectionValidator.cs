using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamSelectionValidator
	{
		bool ValidSelection(IList<IPerson> selectedPersonList, DateOnlyPeriod selectedPeriod);
	}

	public class TeamSelectionValidator : ITeamSelectionValidator
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly IList<IScheduleMatrixPro> _scheduleMatrixList;
 
		public TeamSelectionValidator(ITeamInfoFactory teamInfoFactory, IList<IScheduleMatrixPro> scheduleMatrixList)
		{
			_teamInfoFactory = teamInfoFactory;
			_scheduleMatrixList = scheduleMatrixList;
		}

		public bool ValidSelection(IList<IPerson> selectedPersonList, DateOnlyPeriod selectedPeriod)
		{
			if (selectedPersonList == null) return false;
			if (selectedPersonList.Count == 0) return false;

			foreach (var dateOnly in selectedPeriod.DayCollection())
			{
				var teamInfoList = new HashSet<ITeamInfo>();
				foreach (var selectedPerson in selectedPersonList)
				{
					var teamInfo = _teamInfoFactory.CreateTeamInfo(selectedPerson, dateOnly, _scheduleMatrixList);
					if (teamInfo == null) return false;

					teamInfoList.Add(teamInfo);
				}

				foreach (var teamInfo in teamInfoList)
				{
					var groupPerson = teamInfo.GroupPerson;
					if (groupPerson.GroupMembers.Any(groupMember => !selectedPersonList.Contains(groupMember)))
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
