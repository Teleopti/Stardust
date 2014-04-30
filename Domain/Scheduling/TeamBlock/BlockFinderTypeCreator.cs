using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public static class BlockFinderTypeCreator
    {
        public static  IList<IBlockFinderTypeHelper> GetBlockFinderTypes
        {
            get
            {
                var blockFinderTypes = new List<IBlockFinderTypeHelper>();
                blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.BetweenDayOff.ToString(), Name = UserTexts.Resources.BetweenDayOff });
                blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.SchedulePeriod.ToString(), Name = UserTexts.Resources.SchedulePeriod });
                blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.SingleDay.ToString(), Name = UserTexts.Resources.NoBlock });
                return blockFinderTypes;
            }
            
        }

    }
}
