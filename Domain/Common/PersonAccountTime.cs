using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PersonAccountTime : PersonAccount, IPersonAccountTime
    {
        public PersonAccountTime(DateOnly startDate,IAbsence absence, TimeSpan balanceIn, TimeSpan accrued, TimeSpan extra)
                            :base(startDate, absence, balanceIn, accrued, extra){}

        public PersonAccountTime(DateOnly startDate, IAbsence absence)
            : base(startDate, absence){}

        protected PersonAccountTime()
        {
        }

        public override void Track(TimeSpan timeOrDays)
        {
            LatestCalculatedBalance = timeOrDays;
        }

        #region private methods

        protected IPersonAccount FindEarliestPersonAccount(IList<IPersonAccount> personAccounts)
        {
            return
                 (from p in personAccounts.OfType<IPersonAccountTime>()
                  where (p.TrackingAbsence == null && TrackingAbsence == null) ||
                        (p.TrackingAbsence != null && p.TrackingAbsence.Equals(TrackingAbsence))
                  orderby p.StartDate
                  select p).LastOrDefault(o => o.StartDate < StartDate);
        }

        protected IPersonAccount FindNextPersonAccount(IList<IPersonAccount> personAccounts)
        {
            return
                 (from p in personAccounts.OfType<IPersonAccountTime>()
                  where (p.TrackingAbsence == null && TrackingAbsence == null) ||
                        (p.TrackingAbsence != null && p.TrackingAbsence.Equals(TrackingAbsence))
                  orderby p.StartDate
                  select p).FirstOrDefault(o => o.StartDate > StartDate);
        }

        #endregion //private methods

        public override void CalculateBalanceIn()
        {
            IPersonAccountTime earlierAccount = (IPersonAccountTime)FindEarliestPersonAccount(ParentPerson.PersonAccountCollection);
            BalanceIn = earlierAccount != null ? earlierAccount.BalanceOut : TimeSpan.Zero;

            var nextPersonAccount = FindNextPersonAccount(ParentPerson.PersonAccountCollection);
            if (nextPersonAccount != null) nextPersonAccount.CalculateBalanceIn();
        }
    }
}
