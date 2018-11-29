using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockInfo
	{
		ITeamInfo TeamInfo { get; }
		IBlockInfo BlockInfo { get; }
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndBlock();
		bool AllIsLocked();
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

		public bool AllIsLocked()
		{
			foreach (var scheduleMatrixPro in MatrixesForGroupAndBlock())
			{
				if (scheduleMatrixPro.EffectivePeriodDays.Any(scheduleDayPro => !scheduleMatrixPro.IsDayLocked(scheduleDayPro.Day)))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
        {
            return _teamInfo.GetHashCode() ^ _blockInfo.GetHashCode();
        }

        public override bool Equals(object obj)
        {
			return obj is ITeamBlockInfo ent && Equals(ent);
        }

        public virtual bool Equals(ITeamBlockInfo other)
        {
            if (other == null)
                return false;

            return GetHashCode() == other.GetHashCode();
        }
	}

	public class TeamBlockSingleDayInfo : TeamBlockInfo
	{
		public TeamBlockSingleDayInfo(ITeamInfo teamInfo, DateOnly dateOnly)
			: base(teamInfo, new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly)))
		{
		}
	}
}