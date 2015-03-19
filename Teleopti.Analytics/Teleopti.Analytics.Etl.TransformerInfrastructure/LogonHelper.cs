using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public class LogOnHelper : ILogOnHelper
	{
		private DataSourceContainer _choosenDb;
		private LogOnService _logonService;
		private string _nhibConfPath;
		private IList<IBusinessUnit> _buList;
		private ILogOnOff _logOnOff;
		private IRepositoryFactory _repositoryFactory;
		private List<ITenantName> _tenantNames;
		private List<IDataSource> _dataSources;

		private LogOnHelper() { }

		public LogOnHelper(string hibernateConfPath)
			: this()
		{
			_nhibConfPath = hibernateConfPath;
			initializeStateHolder();
		}

		public IList<IBusinessUnit> GetBusinessUnitCollection()
		{
			if (_buList == null)
			{
				_buList = new List<IBusinessUnit>(_choosenDb.AvailableBusinessUnitProvider.AvailableBusinessUnits());
			}

			//Trace.WriteLine("No allowed business unit found in current database.");
			if (_buList == null || _buList.Count == 0)
			{
				throw new AuthenticationException("No allowed business unit found in current database '" +
											_choosenDb + "'.");
			}

			return _buList;
		}

		public List<ITenantName> TenantCollection
		{
			get
			{
				if (_tenantNames == null || _tenantNames.Count == 0)
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

		private void initializeStateHolder()
		{
			// Code that runs on application startup
			if (string.IsNullOrEmpty(_nhibConfPath))
				_nhibConfPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var dataSourcesFactory = new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(),
				DataSourceConfigurationSetter.ForEtl(), 
				new CurrentHttpContext(),
				() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging
				);
			var application = new InitializeApplication( dataSourcesFactory, null) {MessageBrokerDisabled = true};

			if (!StateHolder.IsInitialized)
				application.Start(new StateManager(), _nhibConfPath, null, new ConfigurationManagerWrapper(), true);

			_logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
			_repositoryFactory = new RepositoryFactory();
			_logonService =
				new LogOnService(_logOnOff);
			_tenantNames = TenantCreator.TenantNames(_nhibConfPath);
			_dataSources = TenantCreator.DataSources(_nhibConfPath, _repositoryFactory, dataSourcesFactory);
		}


		public bool SelectDataSourceContainer(string dataSourceName)
		{
			_buList = null;
			var dataSource = _dataSources.FirstOrDefault(x => x.DataSourceName.Equals(dataSourceName));
			if (dataSource == null)
			{
				Trace.WriteLine(string.Format("No tenant found with name {0}", dataSourceName));
				_choosenDb = null;
			}
			else
			{
				_choosenDb = new DataSourceContainer(dataSource, _repositoryFactory, null, AuthenticationTypeOption.Application);

				using (var unitOfWork = _choosenDb.DataSource.Application.CreateAndOpenUnitOfWork())
				{
					_choosenDb.SetUser(_repositoryFactory.CreatePersonRepository(unitOfWork).LoadPersonAndPermissions(SuperUser.Id_AvoidUsing_This));
				}
				if (_choosenDb.User == null)
				{
					Trace.WriteLine("Error on logon!");
					_choosenDb = null;
				}

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

		public DataSourceContainer ChoosenDataSource
		{
			get { return _choosenDb; }
		}
	}
}