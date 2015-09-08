using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security.Authentication;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class LogOnHelper : ILogOnHelper
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IAvailableBusinessUnitsProvider _availableBusinessUnitsProvider;
		private DataSourceContainer _choosenDb;
		private LogOnService _logonService;
		private IList<IBusinessUnit> _buList;
		private ILogOnOff _logOnOff;
		private List<ITenantName> _tenantNames;

		public LogOnHelper(ILoadAllTenants loadAllTenants, 
									ITenantUnitOfWork tenantUnitOfWork,
									IAvailableBusinessUnitsProvider availableBusinessUnitsProvider)
		{
			_loadAllTenants = loadAllTenants;
			_availableBusinessUnitsProvider = availableBusinessUnitsProvider;
			initializeStateHolder(tenantUnitOfWork);
		}

		public IList<IBusinessUnit> GetBusinessUnitCollection()
		{
			if (_buList == null)
			{
				_buList = new List<IBusinessUnit>(_availableBusinessUnitsProvider.AvailableBusinessUnits(_choosenDb.User, _choosenDb.DataSource));
			}

			if (_buList == null || _buList.Count == 0)
			{
				throw new AuthenticationException("No allowed business unit found in current database '" + _choosenDb + "'.");
			}

			return _buList;
		}

		public List<ITenantName> TenantCollection
		{
			get
			{
				if (_tenantNames.Count == 0)
				{
					throw new DataSourceException("No Tenants found");
				}

				return _tenantNames;
			}
		}

		public IDataSourceContainer SelectedDataSourceContainer { get { return _choosenDb; } }

		public bool SetBusinessUnit(IBusinessUnit businessUnit)
		{
			_logonService.LogOff();
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

		private void initializeStateHolder(ITenantUnitOfWork tenantUnitOfWork)
		{
			// Code that runs on application startup
			var dataSourcesFactory = new DataSourcesFactory(new EnversConfiguration(), new NoMessageSenders(), 
				DataSourceConfigurationSetter.ForEtl(),
				new CurrentHttpContext(),
				() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging
				);
			var dsForTenant = new DataSourceForTenant(dataSourcesFactory);
			var application = new InitializeApplication(dsForTenant, null, _loadAllTenants);

			using (tenantUnitOfWork.Start())
			{
				//RK: why is this if needed!?
				if (!StateHolder.IsInitialized)
					application.Start(new StateManager(), null, ConfigurationManager.AppSettings.ToDictionary(), false);
			}

			_logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
			_logonService =
				new LogOnService(_logOnOff, new AvailableBusinessUnitsProvider(new RepositoryFactory()));
			_tenantNames = new List<ITenantName>();
			StateHolder.Instance.StateReader.ApplicationScopeData.DataSourceForTenant.DoOnAllTenants_AvoidUsingThis(ds =>
			{
				_tenantNames.Add(new TenantName{DataSourceName = ds.DataSourceName});
			});
		}


		public bool SelectDataSourceContainer(string dataSourceName)
		{
			_buList = null;
			var dataSource = StateHolder.Instance.StateReader.ApplicationScopeData.DataSourceForTenant.Tenant(dataSourceName);
			var person = new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(dataSource.Application, SuperUser.Id_AvoidUsing_This);
			_choosenDb = new DataSourceContainer(dataSource, person);
			if (_choosenDb.User == null)
			{
				Trace.WriteLine("Error on logon!");
				_choosenDb = null;
			}

			return _choosenDb != null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				ReleaseManagedResources();
			}
			ReleaseUnmanagedResources();
		}

		protected virtual void ReleaseUnmanagedResources()
		{

		}

		protected virtual void ReleaseManagedResources()
		{
			if (_logonService != null)
			{
				_logonService.LogOff();
				_logonService = null;
			}
			if (_buList != null)
			{
				_buList.Clear();
				_buList = null;
			}
		}

		public void LogOff()
		{
			_logonService.LogOff();
		}
	}
}