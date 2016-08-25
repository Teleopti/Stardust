
using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class CheckingPersonalAccountDaysProvider : ICheckingPersonalAccountDaysProvider
	{
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

		public CheckingPersonalAccountDaysProvider(IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
		}

		public IEnumerable<DateOnly> GetDays(IAbsence absence,
			IPerson person, DateTimePeriod period)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var startDate = new DateOnly(period.StartDateTimeLocal(timeZone));
			var endDate = new DateOnly(period.EndDateTimeLocal(timeZone));

			if (startDate == endDate)
			{
				return new[] { startDate };
			}

			var personAccounts = _personAbsenceAccountRepository.Find(person);
			var days = period.ToDateOnlyPeriod(timeZone).DayCollection();
			var checkedScheduleDays = new HashSet<DateOnly>();
			var checkedAccounts = new HashSet<IAccount>();

			foreach (var day in days)
			{
				var account = personAccounts.Find(absence, day);

				if (account == null)
					continue;

				if (checkedAccounts.Add(account))
					checkedScheduleDays.Add(day);
			}

			return checkedScheduleDays;
		}
	}
}
