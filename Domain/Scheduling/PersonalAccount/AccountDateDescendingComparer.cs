using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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
