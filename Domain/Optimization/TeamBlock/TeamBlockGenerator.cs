using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockGenerator
	{
		IList<ITeamBlockInfo> Generate(IList<IScheduleMatrixPro> allPersonMatrixList,
		                               DateOnlyPeriod selectedPeriod,
		                               IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions);
	}

	public class TeamBlockGenerator : ITeamBlockGenerator
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
	    private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

	    public TeamBlockGenerator(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
	        _teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IList<ITeamBlockInfo> Generate(IList<IScheduleMatrixPro> allPersonMatrixList,
		                                      DateOnlyPeriod selectedPeriod,
		                                      IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions)
		{
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = _teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod,
				                                               allPersonMatrixList);
				if (teamInfo != null)
					allTeamInfoListOnStartDate.Add(teamInfo);
			}

			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					if(!selectedPersons.Contains(groupMember))
						teamInfo.LockMember(groupMember);
				}
			}

			var allTeamBlocksInHashSet = new HashSet<ITeamBlockInfo>();
			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var day in selectedPeriod.DayCollection())
				{
					var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, day,
						schedulingOptions.BlockFinderTypeForAdvanceScheduling,
						_teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions));
					
					if (teamBlock == null) continue;
					allTeamBlocksInHashSet.Add(teamBlock);
				}
			}
			return allTeamBlocksInHashSet.ToList();
		}
	}
}