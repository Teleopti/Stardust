using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class NextPeriodOptimizer : IFilterOnPeriodOptimizer
    {
        private int lastIndex;

        public int FindStartIndex(IEnumerable<IVisualLayer> unmergedCollection, DateTime start)
        {
            if (lastIndex > 0)
            {
                if (unmergedCollection.ElementAt(lastIndex).Period.StartDateTime > start)
                    lastIndex = 0;
            }
            return lastIndex;
        }

        public void FoundEndIndex(int index)
        {
            lastIndex = index;
        }
    }
}
