using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.PersonalAccount
{
    /// <summary>
    /// Closes the <see cref="IAccount"/>
    /// </summary>
    public interface IPersonAccountCloser
    {
        bool ClosePersonAccount(IPersonAccountCollection personAccount, IAbsence absence, DateOnly dateOnly);
    }

    public class PersonAccountCloser : IPersonAccountCloser
    {
        public bool ClosePersonAccount(IPersonAccountCollection personAccount, IAbsence absence, DateOnly dateOnly)
        {

            IAccount thisAccount = personAccount.Find(absence, dateOnly);
            if (thisAccount == null)
                return false;

            IAccount earlierAccount = personAccount.Find(absence, thisAccount.StartDate.AddDays(-1));
            if (earlierAccount == null)
            {
                //thisAccount.BalanceIn = TimeSpan.Zero;
                thisAccount.BalanceOut += thisAccount.Remaining;
                return true;
            }

            TimeSpan newBalanceOut = earlierAccount.BalanceOut + earlierAccount.Remaining;
            earlierAccount.BalanceOut = newBalanceOut;
            thisAccount.BalanceIn = newBalanceOut;

            return true;
        }

        //public virtual IAccount FindPreviousPersonAccount(IEnumerable<IAccount> accounts, IAccount thisAccount)
        //{
        //    IOrderedEnumerable<IAccount> descendingList = accounts.OrderByDescending(account => account.StartDate);
        //    return descendingList.FirstOrDefault(o => o.StartDate < thisAccount.StartDate && o.Owner.);
        //}

    }
}
