using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public class BlockFinderTypeCreator
    {
        public IList<IBlockFinderTypeHelper> GetBlockFinderTypes()
        {
            var blockFinderTypes = new List<IBlockFinderTypeHelper>();
            blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.BetweenDayOff.ToString(), Name = "Between Daysoff" });
            blockFinderTypes.Add(new BlockFinderTypeHelper { Key = BlockFinderType.SchedulePeriod .ToString(), Name = "Schedule Period" });
            return blockFinderTypes;
        }
    }
}
