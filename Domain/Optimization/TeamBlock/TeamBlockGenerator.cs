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

		public TeamBlockGenerator(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
		}

		public IList<ITeamBlockInfo> Generate(IList<IScheduleMatrixPro> allPersonMatrixList,
		                                      DateOnlyPeriod selectedPeriod,
		                                      IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions)
		{
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod,
				                                                               allPersonMatrixList));
			}
			var allTeamBlocksInHashSet = new HashSet<ITeamBlockInfo>();
			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var day in selectedPeriod.DayCollection())
				{
					var teamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, day,
					                                                          schedulingOptions.BlockFinderTypeForAdvanceScheduling);
					allTeamBlocksInHashSet.Add(teamBlock);
				}
			}
			return allTeamBlocksInHashSet.ToList();
		}
	}
}