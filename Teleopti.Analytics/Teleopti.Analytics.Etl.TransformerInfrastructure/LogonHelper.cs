using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using log4net;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public class LogOnHelper : ILogOnHelper, ILicenseFeedback
	{
		private DataSourceContainer _choosenDb;
		private IList<DataSourceContainer> _foundDataBases = new List<DataSourceContainer>();
		private string _nhibConfPath;
		//private readonly string _password;
		//private readonly string _userName;
		private IList<IBusinessUnit> _buList;
		private ILogOnOff _logOnOff;
		private IRepositoryFactory _repositoryFactory;
		private readonly ILog _logger = LogManager.GetLogger(typeof(LogOnHelper));

		private LogOnHelper() { }

		public LogOnHelper(string hibernateConfPath)
			: this()
		{
			//after we added a logon screen so we can decide database in multitenant environment we probably need these again
			//_userName = userName;
			//_password = password;
			_nhibConfPath = hibernateConfPath;

			InitializeStateHolder();
			setDatabase();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
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

		public bool LogOn(IBusinessUnit businessUnit)
		{
			if (_choosenDb != null)
			{
				var licenseVerifier = new LicenseVerifier(this, _choosenDb.DataSource.Application, new LicenseRepository(_choosenDb.DataSource.Application));
				var licenseService = licenseVerifier.LoadAndVerifyLicense();
				if (licenseService == null)
					return false;

				_logOnOff.LogOff();
				_logOnOff.LogOn(_choosenDb.DataSource, _choosenDb.User, businessUnit);
				LicenseActivator.ProvideLicenseActivator();
				return true;
			}

			return false;
		}

		private void InitializeStateHolder()
		{
			// Code that runs on application startup
			if (string.IsNullOrEmpty(_nhibConfPath))
				_nhibConfPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var application =
				new InitializeApplication(
					new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(),
												  DataSourceConfigurationSetter.ForEtl(), new CurrentHttpContext()), null) {MessageBrokerDisabled = true};

			if (!StateHolderReader.IsInitialized)
				application.Start(new StateManager(), _nhibConfPath, null, new ConfigurationManagerWrapper(), true);

			//This one would benefit from some Autofac maybe?
			_logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));

			_repositoryFactory = new RepositoryFactory();
			
		}

		private void setDatabase()
		{
			var dataSourceProvider =
				new AvailableDataSourcesProvider(new ThisApplicationData(StateHolderReader.Instance.StateReader.ApplicationScopeData));
			var datasourceprovider = new ApplicationDataSourceProvider(dataSourceProvider,new RepositoryFactory(), null);
			_foundDataBases = datasourceprovider.DataSourceList().ToList();

			if (_foundDataBases.IsEmpty())
			{
				Trace.WriteLine("Login Failed! could not be found in any database configuration.");
				_choosenDb = null;
			}
			else
			{
				// If multiple databases we use the first in the list, since it is decided that the ETL Tool not should support multi db
				_choosenDb = _foundDataBases.First();
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
			if (_logOnOff != null)
			{
				_logOnOff.LogOff();
				_logOnOff = null;
			}
			if (_buList != null)
			{
				_buList.Clear();
				_buList = null;
			}
			if (_foundDataBases != null)
			{
				_foundDataBases.Clear();
				_foundDataBases = null;
			}
		}

		public void LogOff()
		{
			_logOnOff.LogOff();
		}

		public DataSourceContainer ChoosenDataSource
		{
			get { return _choosenDb; }
		}

		public void Warning(string warning)
		{
			Warning(warning, "");
		}

		public void Warning(string warning, string caption)
		{
			_logger.Warn(warning);
		}

		public void Error(string error)
		{
			_logger.Error(error);
		}
	}
}