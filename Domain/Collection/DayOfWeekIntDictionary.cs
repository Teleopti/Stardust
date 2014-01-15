using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Collection
{
    public class DayOfWeekIntDictionary : ReadOnlyDictionary<DayOfWeek, int>
    {
        public DayOfWeekIntDictionary()
            : base(new Dictionary<DayOfWeek, int>
                       {
                           {DayOfWeek.Monday, 0},
                           {DayOfWeek.Tuesday, 0},
                           {DayOfWeek.Wednesday, 0},
                           {DayOfWeek.Thursday, 0},
                           {DayOfWeek.Friday, 0},
                           {DayOfWeek.Saturday, 0},
                           {DayOfWeek.Sunday, 0}
                       }
            )
        {}

        public override int this[DayOfWeek key]
        {
            get
            {
                if(WrappedDictionary.ContainsKey(key))
                    return WrappedDictionary[key];

                return 0;
            }
            set
            {
                WrappedDictionary[key] = value;
            }
        }

    }
}
