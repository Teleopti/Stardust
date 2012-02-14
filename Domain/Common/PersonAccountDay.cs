using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PersonAccountDay : PersonAccount, IPersonAccountDay
    {
        public PersonAccountDay(DateOnly startDate, IAbsence absence)
            : base(startDate, absence){}

        public PersonAccountDay(DateOnly startDate, IAbsence absence, int balanceIn, int accrued, int extra)
            : base(startDate, absence, TimeSpan.FromDays(balanceIn), TimeSpan.FromDays(accrued), TimeSpan.FromDays(extra)) {}

        protected PersonAccountDay(){}

        public override void Track(TimeSpan timeOrDays)
        {
            LatestCalculatedBalance = timeOrDays; 
        }

        protected IPersonAccount FindEarliestPersonAccount(IList<IPersonAccount> personAccounts)
        {
            return
                 (from p in personAccounts.OfType<IPersonAccountDay>()
                  where (p.TrackingAbsence == null && TrackingAbsence == null) ||
                        (p.TrackingAbsence != null && p.TrackingAbsence.Equals(TrackingAbsence))
                  orderby p.StartDate
                  select p).LastOrDefault(o => o.StartDate < StartDate);
        }

        protected IPersonAccount FindNextPersonAccount(IList<IPersonAccount> personAccounts)
        {
            return
                 (from p in personAccounts.OfType<IPersonAccountDay>()
                  where (p.TrackingAbsence == null && TrackingAbsence == null) ||
                        (p.TrackingAbsence != null && p.TrackingAbsence.Equals(TrackingAbsence))
                  orderby p.StartDate
                  select p).FirstOrDefault(o => o.StartDate > StartDate);
        }

        public override void CalculateBalanceIn()
        {
            IPersonAccountDay earlierAccount = (IPersonAccountDay)FindEarliestPersonAccount(ParentPerson.PersonAccountCollection);
            BalanceIn = earlierAccount != null ? earlierAccount.BalanceOut : TimeSpan.Zero;

            var nextPersonAccount = FindNextPersonAccount(ParentPerson.PersonAccountCollection);
            if (nextPersonAccount != null) nextPersonAccount.CalculateBalanceIn();
        }
    }
}
