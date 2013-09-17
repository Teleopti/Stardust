using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Teleopti.Ccc.DayOffPlanning
{
    public interface IWorkShiftLegalStateDayIndexCalculator
    {
        /// <summary>
        /// Calculates the indexes for raising the working hours in a shift.
        /// </summary>
        /// <param name="relativeDeficits">The relative deficits.</param>
        /// <returns></returns>
        ReadOnlyCollection<double?> CalculateIndexForRaising(IEnumerable<double?> relativeDeficits);

        /// <summary>
        /// Calculates the index for reducing the working hours in a shift.
        /// </summary>
        /// <param name="relativeDeficits">The relative deficits.</param>
        /// <returns></returns>
        ReadOnlyCollection<double?> CalculateIndexForReducing(IEnumerable<double?> relativeDeficits);
    }

    public class WorkShiftLegalStateDayIndexCalculator : IWorkShiftLegalStateDayIndexCalculator
    {
        public ReadOnlyCollection<double?> CalculateIndexForRaising(IEnumerable<double?> relativeDeficits)
        {
                List<double> list = (from relativeDeficit in relativeDeficits
                                     where relativeDeficit.HasValue
                                     select relativeDeficit.Value).ToList();

                var max = 0d;
                if(list.Count > 0) max = list.Max();

                IList<double?> retList = new Collection<double?>();
                foreach (double? deficit in relativeDeficits)
                {
                    if(deficit.HasValue)
                        retList.Add(Math.Abs(deficit.Value - max - 1));
                    else
                        retList.Add(deficit);
                    
                }
                return new ReadOnlyCollection<double?>(retList);
        }

        public ReadOnlyCollection<double?> CalculateIndexForReducing(IEnumerable<double?> relativeDeficits)
        {
            List<double> list = (from relativeDeficit in relativeDeficits
                                 where relativeDeficit.HasValue
                                 select relativeDeficit.Value).ToList();

            var min = 0d;
            if(list.Count > 0) min = Math.Abs(list.Min());

            IList<double?> retList = new Collection<double?>();
            foreach (double? deficit in relativeDeficits)
            {
                if (deficit.HasValue)
                    retList.Add(deficit.Value + min + 1);
                else
                    retList.Add(deficit);
            }
            return new ReadOnlyCollection<double?>(retList);
        }
    }
}
