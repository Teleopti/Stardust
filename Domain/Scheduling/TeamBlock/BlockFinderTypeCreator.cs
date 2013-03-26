using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.BetweenDayOff.ToString(), Name = "Between Daysoff" });
                blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.SchedulePeriod.ToString(), Name = "Schedule Period" });
                return blockFinderTypes;
            }
            
        }

    }
}
