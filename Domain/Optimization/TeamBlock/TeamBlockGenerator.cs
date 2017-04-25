using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockGenerator
	{
		IList<ITeamBlockInfo> Generate(IEnumerable<IPerson> personsInOrganisation,
																		IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
		                               DateOnlyPeriod selectedPeriod,
		                               IEnumerable<IPerson> selectedPersons, SchedulingOptions schedulingOptions);
	}

	public class TeamBlockGenerator : ITeamBlockGenerator
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;

	    public TeamBlockGenerator(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IList<ITeamBlockInfo> Generate(IEnumerable<IPerson> personsInOrganisation,
																					IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
		                                      DateOnlyPeriod selectedPeriod,
																					IEnumerable<IPerson> selectedPersons, SchedulingOptions schedulingOptions)
		{
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = _teamInfoFactory.CreateTeamInfo(personsInOrganisation, selectedPerson, selectedPeriod,
				                                               allPersonMatrixList);
				if (teamInfo != null)
					allTeamInfoListOnStartDate.Add(teamInfo);
			}

			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					if(!selectedPersons.Contains(groupMember))
						teamInfo.LockMember(selectedPeriod, groupMember);
				}
			}

			var allTeamBlocksInHashSet = new HashSet<ITeamBlockInfo>();
			var daysInSelectedPeriod = selectedPeriod.DayCollection();
			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var day in daysInSelectedPeriod)
				{
					var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, day,
						schedulingOptions.BlockFinder());
					
					if (teamBlock == null) continue;
					allTeamBlocksInHashSet.Add(teamBlock);
				}
			}
			return allTeamBlocksInHashSet.ToList();
		}
	}
}