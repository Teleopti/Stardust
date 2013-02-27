

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IBlockInfo
	{
		DateOnlyPeriod BlockPeriod { get; }
	}

	public class BlockInfo : IBlockInfo
	{
		private readonly DateOnlyPeriod _blockPeriod;

		public BlockInfo(DateOnlyPeriod blockPeriod)
		{
			_blockPeriod = blockPeriod;
		}

		public DateOnlyPeriod BlockPeriod
		{
			get { return _blockPeriod; }
		}
	}
}