

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockInfo
	{
		ITeamInfo TeamInfo { get; }
		IBlockInfo BlockInfo { get; }
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndBlock();
	}

	public class TeamBlockInfo : ITeamBlockInfo
	{
		private readonly ITeamInfo _teamInfo;
		private readonly IBlockInfo _blockInfo;

		public TeamBlockInfo(ITeamInfo teamInfo, IBlockInfo blockInfo)
		{
			_teamInfo = teamInfo;
			_blockInfo = blockInfo;
		}

		public ITeamInfo TeamInfo
		{
			get { return _teamInfo; }
		}

		public IBlockInfo BlockInfo
		{
			get { return _blockInfo; }
		}

		public IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndBlock()
		{
			return TeamInfo.MatrixesForGroupAndPeriod(BlockInfo.BlockPeriod);
		}
	}
}