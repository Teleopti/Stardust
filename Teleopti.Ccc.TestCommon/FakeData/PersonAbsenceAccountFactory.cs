using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PersonAbsenceAccountFactory
	{
		public static PersonAbsenceAccount CreatePersonAbsenceAccount(IPerson person, IAbsence absence,
			params IAccount[] accountDays)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			foreach (var accountDay in accountDays)
			{
				personAbsenceAccount.Add(accountDay);
			}
			return personAbsenceAccount;
		}
	}
}
