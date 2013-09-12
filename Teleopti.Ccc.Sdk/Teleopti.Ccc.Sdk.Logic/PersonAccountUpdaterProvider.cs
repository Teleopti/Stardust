using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic
{
	public class PersonAccountUpdaterProvider : IPeopleAccountUpdaterProvider
	{

		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		private IUnitOfWork _unitOfWork;
		private readonly ITraceableRefreshService _refreshService;
		private readonly object Locker = new object();

		public PersonAccountUpdaterProvider(
			IRepositoryFactory repositoryFactory,
			ITraceableRefreshService refreshService)
		{
			_repositoryFactory = repositoryFactory;
			_refreshService = refreshService;
		}

		public ITraceableRefreshService RefreshService
		{
			get { return _refreshService; }
		}

		public IUnitOfWork UnitOfWork
		{
			get 
			{
				lock (Locker)
				{
					if (_unitOfWork == null)
						_unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
					return _unitOfWork;
				}
			}
		}

		public IPersonAccountCollection PersonAccounts(IPerson person)
		{
			var personAccountRepository = _repositoryFactory.CreatePersonAbsenceAccountRepository(UnitOfWork);
			var accounts = personAccountRepository.Find(person);
			return accounts;
		}
	}
}