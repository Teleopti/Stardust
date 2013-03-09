using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface IBlockProvider
	{
		IList<IBlockInfo> Provide(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
		                          IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulingOptions schedulingOptions);
	}

	public class BlockProvider : IBlockProvider
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;

		public BlockProvider(ITeamInfoFactory teamInfoFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory)
		{
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
		}

		public IList<IBlockInfo> Provide(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulingOptions schedulingOptions)
		{
			var blocks = new List<IBlockInfo>();
			foreach (var datePointer in selectedPeriod.DayCollection())
			{
				var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
				foreach (var selectedPerson in selectedPersons)
				{
					allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, datePointer, allPersonMatrixList));
				}

				foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
				{

					ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
																							 schedulingOptions
																								 .BlockFinderTypeForAdvanceScheduling);
					if (blocks.Any(x => x.BlockPeriod.Equals(teamBlockInfo.BlockInfo.BlockPeriod))) continue;
					blocks.Add(new BlockInfo(teamBlockInfo.BlockInfo.BlockPeriod));
				}
			}
			return blocks;
		}
	}
}