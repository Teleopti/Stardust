using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{

    /// <summary>
    /// An IFilterOnPeriodOptimizer to use
    /// if no optmization should be used.
    /// Should not be used in production code.
    /// Here primary to be able to the VisualLayerCollection
    /// with no optimizer at all.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-02-04
    /// </remarks>
    [IsNotDeadCode]
    public class FilterLayerNoOptimizer : IFilterOnPeriodOptimizer
    {
        public int FindStartIndex(IEnumerable<IVisualLayer> unmergedCollection, DateTime start)
        {
            return 0;
        }

        public void FoundEndIndex(int index)
        {
        }
    }
}
