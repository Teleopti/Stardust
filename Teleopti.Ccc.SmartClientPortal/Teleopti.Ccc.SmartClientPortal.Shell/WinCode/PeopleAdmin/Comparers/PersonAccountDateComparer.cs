using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    public class PersonAccountDateComparer : IComparer<IPersonAccountModel>
    {
        public int Compare(IPersonAccountModel x, IPersonAccountModel y)
        {
            int result = 0;

            if (x.AccountDate.HasValue && y.AccountDate.HasValue)
            {
                result = DateTime.Compare(x.AccountDate.Value.Date, y.AccountDate.Value.Date);
            }
            else if (!x.AccountDate.HasValue && y.AccountDate.HasValue)
            {
                result = -1;
            }
            else if (!y.AccountDate.HasValue && x.AccountDate.HasValue)
            {
                result = 1;
            }

	        return result;
        }
    }
}
