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
    [IsNotDeadCode("Keep this for now - used when doing performance benchmarks with projections.")]
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
