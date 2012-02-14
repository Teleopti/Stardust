using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class SmartJumpOptimizer : IFilterOnPeriodOptimizer
    {
        public SmartJumpOptimizer(int endSize)
        {
            InParameter.ValueMustBeLargerThanZero("endSize", endSize);
            EndSize = endSize;
        }

        public int EndSize { get; private set; }

        public int FindStartIndex(IEnumerable<IVisualLayer> unmergedCollection, DateTime start)
        {
            int startIndex = 0;
            int endIndex = unmergedCollection.Count() - 1;
            int tryIndex = (endIndex - startIndex) / 2;

            if (endIndex < 2)
                return 0;

            while ((endIndex - startIndex) > EndSize)
            {
                var period = unmergedCollection.ElementAt(tryIndex).Period;
                if (period.StartDateTime >= start)
                {
                    endIndex = tryIndex;
                }
                else
                {
                    if (period.EndDateTime < start)
                    {
                        startIndex = tryIndex;
                    }
                    else
                    {
                        break;
                    }
                }
                tryIndex = startIndex + ((endIndex - startIndex) / 2);
            }

            return startIndex;
        }

        public void FoundEndIndex(int index)
        {
            //uninterested for this optimizer
        }
    }
}
