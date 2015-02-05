using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
		private IList<DataSourceContainer> _foundDataBases = new List<DataSourceContainer>();
		private LogOnService _logonService;
		private string _nhibConfPath;
		private IList<IBusinessUnit> _buList;
		private ILogOnOff _logOnOff;
		private IRepositoryFactory _repositoryFactory;
		private List<DataSourceContainer> _dataSources;

		private LogOnHelper() { }

		public LogOnHelper(string hibernateConfPath)
			: this()
		{
			_nhibConfPath = hibernateConfPath;
			InitializeStateHolder();
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

		public IList<DataSourceContainer> GetDataSourceCollection()
		{
			if (_dataSources == null)
			{
				var dataSourceProvider =
				new AvailableDataSourcesProvider(new ThisApplicationData(StateHolderReader.Instance.StateReader.ApplicationScopeData));
				var datasourceprovider = new ApplicationDataSourceProvider(dataSourceProvider, new RepositoryFactory(), null);
				_dataSources = datasourceprovider.DataSourceList().ToList();
				
			}

			if (_dataSources == null || _dataSources.Count == 0)
			{
				throw new DataSourceException("No datasources found");
			}

			return _dataSources;
		}

		public IDataSourceContainer SelectedDataSourceContainer { get { return _choosenDb; }}

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

		

		/// <summary>
		/// Initializes the state holder components.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void InitializeStateHolder()
		{
			// Code that runs on application startup
			if (string.IsNullOrEmpty(_nhibConfPath))
                _nhibConfPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var application =
				new InitializeApplication(
					new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(),
					                       DataSourceConfigurationSetter.ForEtl(), new CurrentHttpContext()), null);
			application.MessageBrokerDisabled = true;

			if (!StateHolder.IsInitialized)
				application.Start(new StateManager(), _nhibConfPath, null, new ConfigurationManagerWrapper(), true);

			//This one would benefit from some Autofac maybe?
			_logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
			_repositoryFactory = new RepositoryFactory();
			var passwordPolicy = new DummyPasswordPolicy();
			_logonService =
				new LogOnService(
					new ApplicationDataSourceProvider(
						new AvailableDataSourcesProvider(
							new ThisApplicationData(StateHolderReader.Instance.StateReader.ApplicationScopeData)),
						_repositoryFactory,
						new FindApplicationUser(
							new CheckNullUser(
								new FindUserDetail(
									new CheckUserDetail(new CheckPassword(new OneWayEncryption(), new CheckBruteForce(passwordPolicy),
										new CheckPasswordChange(() => passwordPolicy))), _repositoryFactory)), _repositoryFactory)),_logOnOff);
		}


		public bool SelectDataSourceContainer(string dataSourceName)
		{
			_buList = null;
			if (GetDataSourceCollection().IsEmpty())
			{
				Trace.WriteLine("Login Failed! could not be found in any database configuration.");
				_choosenDb = null;
			}
			else
			{
				// If multiple databases we use the first in the list, since it is decided that the ETL Tool not should support multi db
				_choosenDb = GetDataSourceCollection().FirstOrDefault(x => x.DataSourceName.Equals(dataSourceName));
				if (_choosenDb == null)
					Trace.WriteLine(string.Format("No datasource found with name {0}", dataSourceName));
				else
				{
					using (var unitOfWork = _choosenDb.DataSource.Application.CreateAndOpenUnitOfWork())
					{
						var systemId = new Guid("3f0886ab-7b25-4e95-856a-0d726edc2a67");
						_choosenDb.SetUser(_repositoryFactory.CreatePersonRepository(unitOfWork).LoadOne(systemId));
					}
					if (_choosenDb.User == null)
					{
						Trace.WriteLine("Error on logon!");
						_choosenDb = null;
					}
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
			if (_buList!=null)
			{
				_buList.Clear();
				_buList = null;
			}
			if (_foundDataBases!=null)
			{
				_foundDataBases.Clear();
				_foundDataBases = null;
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