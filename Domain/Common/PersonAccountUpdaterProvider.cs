using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public class PersonAccountUpdaterProvider : IPeopleAccountUpdaterProvider
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPersonAccountCollection _personAccounts;
		private readonly ITraceableRefreshService _refreshService;

		public PersonAccountUpdaterProvider(
			IUnitOfWork unitOfWork,
			IPersonAccountCollection personAccounts,
			ITraceableRefreshService refreshService)
		{
			_unitOfWork = unitOfWork;
			_personAccounts = personAccounts;
			_refreshService = refreshService;
		}

		public ITraceableRefreshService RefreshService
		{
			get { return _refreshService; }
		}

		public IUnitOfWork UnitOfWork
		{
			get { return _unitOfWork; }
		}

		public IPersonAccountCollection PersonAccounts(IPerson person)
		{
			//PersonAbsenceAccountRepository
			//var accounts = _personAbsenceAccountRepository.Find(foundPerson);
			////var personAccount = accounts.Find(foundAbsence, dateFrom);
			return new PersonAccountCollection(person);
			//return _personAccounts.Where(a => a.Key == person);
		}
	}
}