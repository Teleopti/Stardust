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
                blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.BetweenDayOff.ToString(), Name = "Between days off" });
                blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.SchedulePeriod.ToString(), Name = "Schedule Period" });
                return blockFinderTypes;
            }
            
        }

    }
}
