using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class PersonAccountCollectionFactory
	{
		public static PersonAccountCollection Create(IPerson person, IAbsence absence1, IAbsence absence2, out AccountDay account1, out AccountTime account2)
		{
			var from1 = new DateOnly(2008, 1, 3);
            var from2 = new DateOnly(2009, 1, 3);
            
            account1 = new AccountDay(from1);
            account1.BalanceIn = TimeSpan.FromDays(5);
            account1.Accrued = TimeSpan.FromDays(20);
            account1.Extra = TimeSpan.FromDays(5);

            account2 = new AccountTime(from2);
            account2.BalanceIn = new TimeSpan(3);

            var apa = new PersonAbsenceAccount(person, absence1);
            apa.Absence.Tracker = Tracker.CreateDayTracker();
            apa.Add(account1);

            var apa2 = new PersonAbsenceAccount(person, absence2);
            apa2.Absence.Tracker = Tracker.CreateTimeTracker();
            apa2.Add(account2);

            return new PersonAccountCollection(person) {apa, apa2}; 
		}
	}
}
