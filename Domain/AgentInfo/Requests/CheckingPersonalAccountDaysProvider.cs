using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class CheckingPersonalAccountDaysProvider : ICheckingPersonalAccountDaysProvider
	{
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

		public CheckingPersonalAccountDaysProvider(IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
		}

		public DateOnlyPeriod GetDays(IAbsence absence, IPerson person, DateTimePeriod period)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);

			if (dateOnlyPeriod.DayCount() == 1)
			{
				return dateOnlyPeriod;
			}

			var personAccounts = _personAbsenceAccountRepository.Find(person);
			var account = personAccounts.Find(absence);
			if (account == null)
				return dateOnlyPeriod.StartDate.ToDateOnlyPeriod();

			var foundAccounts = account.Find(dateOnlyPeriod).ToArray();
			if (foundAccounts.Length == 0)
				return dateOnlyPeriod.StartDate.ToDateOnlyPeriod();

			var firstDate = foundAccounts.OrderBy(a => a.StartDate).First();
			if (firstDate.StartDate <= dateOnlyPeriod.StartDate)
				return dateOnlyPeriod;

			return new DateOnlyPeriod(firstDate.StartDate,dateOnlyPeriod.EndDate);
		}
	}
}
