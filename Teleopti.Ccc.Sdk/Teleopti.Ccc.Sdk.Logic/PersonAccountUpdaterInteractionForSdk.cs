using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic
{
	public class PersonAccountUpdaterSdkProvider : IPersonAccountUpdaterProvider
	{

		private readonly IRepositoryFactory _repositoryFactory;

		public PersonAccountUpdaterSdkProvider(
			IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public ITraceableRefreshService GetRefreshService()
		{
			var scenarioDepository = _repositoryFactory.CreateScenarioRepository(GetUnitOfWork);
			var defaultScenario = scenarioDepository.LoadDefaultScenario();
			return new TraceableRefreshService(defaultScenario, _repositoryFactory);
		}

		public IUnitOfWork GetUnitOfWork
		{
			get { return UnitOfWorkFactory.Current.CurrentUnitOfWork(); }
		}

		public IPersonAccountCollection GetPersonAccounts(IPerson person)
		{
			var personAccountRepository = _repositoryFactory.CreatePersonAbsenceAccountRepository(GetUnitOfWork);
			var accounts = personAccountRepository.Find(person);
			return accounts;
		}
	}
}