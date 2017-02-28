﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.PersonalAccount
{
    //tested from personabsenceaccounttest
    public class AccountDateDescendingComparer : IComparer<IAccount>
    {
        public int Compare(IAccount x, IAccount y)
        {
            return y.StartDate.CompareTo(x.StartDate);
        }
    }
}
