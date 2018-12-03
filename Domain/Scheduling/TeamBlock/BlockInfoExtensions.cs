using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public static class BlockInfoExtensions
	{
		public static DateOnly DatePoint(this IBlockInfo blockInfo, DateOnlyPeriod period)
		{
			//don't really understand but extracted from present code.
			var datePoint = period.StartDate;
			if (blockInfo.BlockPeriod.StartDate > datePoint)
				datePoint = blockInfo.BlockPeriod.StartDate;
				
			return datePoint; 
		}
	}
}