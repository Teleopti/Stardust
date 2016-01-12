using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security.Authentication;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class LogOnHelper : ILogOnHelper
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly IAvailableBusinessUnitsProvider _availableBusinessUnitsProvider;
		private readonly TenantHolder _tenantHolder;
		private DataSourceContainer _choosenDb;
		private LogOnService _logonService;
		private IList<IBusinessUnit> _buList;
		private ILogOnOff _logOnOff;

		public LogOnHelper(
			ILoadAllTenants loadAllTenants, 
			ITenantUnitOfWork tenantUnitOfWork,
			IAvailableBusinessUnitsProvider availableBusinessUnitsProvider,
			TenantHolder tenantHolder)
		{
			_loadAllTenants = loadAllTenants;
			_tenantUnitOfWork = tenantUnitOfWork;
			_availableBusinessUnitsProvider = availableBusinessUnitsProvider;
			_tenantHolder = tenantHolder;
			initializeStateHolder();
			RefreshTenantList();
		}

		public IList<IBusinessUnit> GetBusinessUnitCollection()
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

		public IEnumerable<TenantInfo> TenantCollection
		{
			get
			{
				if (_tenantHolder.LoadedTenants().IsEmpty())
					throw new DataSourceException("No Tenants found");
				return _tenantHolder.LoadedTenants();
			}
		}

		public IDataSourceContainer SelectedDataSourceContainer
		{
			get { return _choosenDb; }
		}

		public bool SetBusinessUnit(IBusinessUnit businessUnit)
		{
			if (_choosenDb != null)
			{
				if (_logonService.LogOn(_choosenDb, businessUnit))
				{
					LicenseActivator.ProvideLicenseActivator();
					return true;
				}
			}
			return false;
		}

		private void initializeStateHolder()
		{
			var application = new InitializeApplication(null);
			application.Start(new State(), null, ConfigurationManager.AppSettings.ToDictionary());

			_logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
			_logonService =
				new LogOnService(_logOnOff, new AvailableBusinessUnitsProvider(new RepositoryFactory()));
		}

		public bool SelectDataSourceContainer(string dataSourceName)
		{
			_buList = null;
			var dataSource = _tenantHolder.DataSourceForTenant(dataSourceName);
			var person = new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(dataSource.Application,
				SuperUser.Id_AvoidUsing_This);
			_choosenDb = new DataSourceContainer(dataSource, person);
			if (_choosenDb.User == null)
			{
				Trace.WriteLine("Error on logon!");
				_choosenDb = null;
			}

			return _choosenDb != null;
		}

		public void RefreshTenantList()
		{
			_tenantHolder.Refresh(_tenantUnitOfWork, _loadAllTenants);
		}

	}
}