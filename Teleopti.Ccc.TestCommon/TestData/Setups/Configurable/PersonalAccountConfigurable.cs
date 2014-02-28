using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonalAccountConfigurable : IUserDataSetup
	{
		public string AbsenceName { get; set; }
		public string Type { get; set; }
		public string Accured { get; set; }
		public string BalanceIn { get; set; }
		public string BalanceOut { get; set; }
		public DateOnly PeriodStartTime { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var absence = new AbsenceRepository(uow).LoadAll().Single(abs => abs.Description.Name.Equals(AbsenceName));
			var account = new AccountDay(PeriodStartTime);
			
			var personAbsenceAccount = new PersonAbsenceAccount(user, absence);
			personAbsenceAccount.Add(account);

			var repository = new PersonAbsenceAccountRepository(uow);
			repository.Add(personAbsenceAccount);
		}
	}
}