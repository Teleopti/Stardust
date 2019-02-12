using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Administration.Core
{
	public class CreateBusinessUnitFake : ICreateBusinessUnit
	{
		public void Create(Tenant tenant, string businessUnitName)
		{
		}
	}

	public interface ICreateBusinessUnit
	{
		void Create(Tenant tenant, string businessUnitName);
	}

	public class CreateBusinessUnit : ICreateBusinessUnit
	{
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly IRunWithUnitOfWork _runWithUnitOfWork;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IRtaStateGroupRepository _rtaStateGroupRepository;
		private readonly InitializeBusinessUnitDatabaseState _initializeBusinessUnitDatabaseState;

		public CreateBusinessUnit(IDataSourcesFactory dataSourcesFactory,
			IRunWithUnitOfWork runWithUnitOfWork,
			IBusinessUnitRepository businessUnitRepository,
			IPersonRepository personRepository,
			IRtaStateGroupRepository rtaStateGroupRepository,
			InitializeBusinessUnitDatabaseState initializeBusinessUnitDatabaseState)
		{
			_dataSourcesFactory = dataSourcesFactory;
			_runWithUnitOfWork = runWithUnitOfWork;
			_businessUnitRepository = businessUnitRepository;
			_personRepository = personRepository;
			_rtaStateGroupRepository = rtaStateGroupRepository;
			_initializeBusinessUnitDatabaseState = initializeBusinessUnitDatabaseState;
		}

		public void Create(Tenant tenant, string businessUnitName)
		{
			var dataSource = _dataSourcesFactory.Create(tenant.Name, tenant.DataSourceConfiguration.ApplicationConnectionString, tenant.DataSourceConfiguration.AnalyticsConnectionString);
			var newBusinessUnit = new BusinessUnit(businessUnitName);
			IPerson systemUser = null;

			_runWithUnitOfWork.WithGlobalScope(dataSource, () =>
			{
				_businessUnitRepository.Add(newBusinessUnit);
				systemUser = _personRepository.Get(SystemUser.Id); // This requires that the system user already exists!
			});

			_runWithUnitOfWork.WithBusinessUnitScope(dataSource, newBusinessUnit, () =>
			{
				_initializeBusinessUnitDatabaseState.Execute(newBusinessUnit, systemUser);

				//shouldnt be here but belongs to RTA stuff
				var rtaStateGroupCreator = new RtaStateGroupCreator(@"RtaStates.xml");
				rtaStateGroupCreator.RtaGroupCollection.ForEach(x => _rtaStateGroupRepository.Add(x));
				//////////////////////////////////////////
			});
		}
	}
}