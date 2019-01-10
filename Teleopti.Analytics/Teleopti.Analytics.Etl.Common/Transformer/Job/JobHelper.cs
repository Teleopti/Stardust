using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security.Authentication;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job
{
	public class JobHelper : IJobHelper
	{
		private IRaptorRepository _repository;
		private readonly IMessageSender _messageSender;
		private readonly IAvailableBusinessUnitsProvider _availableBusinessUnitsProvider;
		private readonly ITenants _tenants;
		private readonly IIndexMaintenanceRepository _indexMaintenanceRepository;
		private readonly IAnalyticsPersonPeriodDateFixer _analyticsPersonPeriodDateFixer;

		private readonly LogOnService _logonService;
		private List<IBusinessUnit> _buList;
		private DataSourceContainer _choosenDb;

		public JobHelper(IAvailableBusinessUnitsProvider availableBusinessUnitsProvider, ITenants tenants, IIndexMaintenanceRepository indexMaintenanceRepository,IMessageSender messageSender, IAnalyticsPersonPeriodDateFixer analyticsPersonPeriodDateFixer)
		{
			_availableBusinessUnitsProvider = availableBusinessUnitsProvider;
			_tenants = tenants;
			_indexMaintenanceRepository = indexMaintenanceRepository;

			_messageSender = messageSender;
			_analyticsPersonPeriodDateFixer = analyticsPersonPeriodDateFixer;

			var application = new InitializeApplication(null);
			application.Start(new State(), null, ConfigurationManager.AppSettings.ToDictionary());
			var logOnOff = new LogOnOff(null, null, null, null, null, new AppDomainPrincipalContext(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()), new ThreadPrincipalContext()), new TeleoptiPrincipalFactory(), null);
			_logonService = new LogOnService(logOnOff, new AvailableBusinessUnitsProvider(new RepositoryFactory()));
		}

		protected JobHelper(IRaptorRepository repository, IMessageSender messageSender, ITenants tenants, IIndexMaintenanceRepository indexMaintenanceRepository, IAnalyticsPersonPeriodDateFixer analyticsPersonPeriodDateFixer)
		{
			_repository = repository;
			_messageSender = messageSender;
			_tenants = tenants;
			_indexMaintenanceRepository = indexMaintenanceRepository;
			_analyticsPersonPeriodDateFixer = analyticsPersonPeriodDateFixer;
		}

		public IRaptorRepository Repository
		{
			get { return _repository; }
		}

		public IMessageSender MessageSender
		{
			get { return _messageSender; }
		}

		public void LogOffTeleoptiCccDomain()
		{
			_repository = null;
		}

		public IList<IBusinessUnit> BusinessUnitCollection
		{
			get
			{
				if (_buList == null)
				{
					_buList =
						new List<IBusinessUnit>(_availableBusinessUnitsProvider.AvailableBusinessUnits(_choosenDb.User,
							_choosenDb.DataSource));
				}

				if (_buList == null || _buList.Count == 0)
				{
					throw new AuthenticationException("No allowed business unit found in current database '" + _choosenDb + "'.");
				}

				return _buList;
			}
		}

		public virtual bool SetBusinessUnit(IBusinessUnit businessUnit)
		{
			if (_choosenDb != null)
			{
				if (_logonService.LogOn(_choosenDb, businessUnit))
				{
					LicenseActivator.ProvideLicenseActivator();

					//Create repository when logged in to raptor domain
					_repository = new RaptorRepository(
						SelectedDataSourceContainer.DataSource.Analytics.ConnectionString, _indexMaintenanceRepository, _analyticsPersonPeriodDateFixer);
					return true;
				}
			}
			return false;
		}

		public virtual bool SelectDataSourceContainer(string dataSourceName)
		{
			_buList = null;
			var dataSource = _tenants.DataSourceForTenant(dataSourceName);
			var person = new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(dataSource.Application,
				SystemUser.Id);
			_choosenDb = new DataSourceContainer(dataSource, person);
			if (_choosenDb.User == null)
			{
				Trace.WriteLine("Error on logon!");
				_choosenDb = null;
			}

			return _choosenDb != null;
		}

		public IDataSource SelectedDataSource
		{
			get { return SelectedDataSourceContainer.DataSource; }
		}

		public IDataSourceContainer SelectedDataSourceContainer
		{
			get { return _choosenDb; }
		}


	}
}