using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public static class BlockInfoExtensions
	{
		public static DateOnly DatePoint(this IBlockInfo blockInfo, DateOnlyPeriod period)
		{
			//don't really understand but extracted from present code.
			return blockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= period.StartDate); 
		}
	}
}