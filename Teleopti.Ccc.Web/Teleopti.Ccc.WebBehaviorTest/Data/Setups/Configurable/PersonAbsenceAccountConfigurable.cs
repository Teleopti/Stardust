using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonAbsenceAccountConfigurable : IUserDataSetup
	{
		public string Absence { get; set; }
		public string FromDate { get; set; }
		public string TrackerType { get; set; }

		public string Accrued { get; set; }
		public string BalanceIn { get; set; }
		public string BalanceOut { get; set; }
		public string Extra { get; set; }

		public PersonAbsenceAccountConfigurable()
		{
			Extra = "0";
			BalanceIn = "0";
			BalanceOut = "0";
			TrackerType = "Days";
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var absence = new AbsenceRepository(uow).LoadAll().Single(x => x.Description.Name == Absence);

			var repository = new PersonAbsenceAccountRepository(uow);
			var personAbsenceAccount = new PersonAbsenceAccount(user, absence);
			switch (TrackerType.ToLower())
			{
				case "days":
					personAbsenceAccount.Add(new AccountDay(FromDate)
						{
							Accrued = TimeSpan.Parse(Accrued),
							BalanceIn = TimeSpan.Parse(BalanceIn),
							BalanceOut = TimeSpan.Parse(BalanceOut),
							Extra = TimeSpan.Parse(Extra)
						});
					break;
				case "hours":
					personAbsenceAccount.Add(new AccountTime(FromDate)
						{
							Accrued = TimeSpan.Parse(Accrued),
							BalanceIn = TimeSpan.Parse(BalanceIn),
							BalanceOut = TimeSpan.Parse(BalanceOut),
							Extra = TimeSpan.Parse(Extra)
						});
					break;
			}
			repository.Add(personAbsenceAccount);
		}
	}
}
