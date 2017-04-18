using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
    public class SchedulePeriodMustHavePreferenceComparer : IComparer<SchedulePeriodModel>
    {
        public int Compare(SchedulePeriodModel x, SchedulePeriodModel y)
        {
            if(x == null) throw new ArgumentNullException("x");
            if(y == null) throw new ArgumentNullException("y");

            var result = 0;

            if (x.MustHavePreference == 0 && y.MustHavePreference == 0)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.MustHavePreference == 0)
            {
                result = -1;
            }
            else if (y.MustHavePreference == 0)
            {
                result = 1;
            }
            else
            {
                if (x.MustHavePreference < y.MustHavePreference)
                {
                    result = -1;
                }
                if (x.MustHavePreference > y.MustHavePreference)
                {
                    result = 1;
                }
            }

            return result;
        }
    }
}
