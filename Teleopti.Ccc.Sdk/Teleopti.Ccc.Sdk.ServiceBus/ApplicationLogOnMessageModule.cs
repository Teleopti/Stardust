using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security;
using log4net;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.MessageModules;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class ApplicationLogOnMessageModule : IMessageModule
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ApplicationLogOnMessageModule));
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly DataSourceForTenantWrapper _dataSourceForTenant;
		private readonly AsSuperUser _asSuperUser;

		public ApplicationLogOnMessageModule(ILogOnOff logOnOff,
			IRoleToClaimSetTransformer roleToClaimSetTransformer,
			IRepositoryFactory repositoryFactory,
			DataSourceForTenantWrapper dataSourceForTenant)
		{
			_repositoryFactory = repositoryFactory;
			_dataSourceForTenant = dataSourceForTenant;
			_asSuperUser = new AsSuperUser(logOnOff, repositoryFactory, roleToClaimSetTransformer);
		}

		public void Init(ITransport transport, IServiceBus bus)
		{
			transport.MessageArrived += transport_MessageArrived;
			transport.MessageProcessingCompleted += transport_MessageProcessingCompleted;
		}

		void transport_MessageProcessingCompleted(Rhino.ServiceBus.Impl.CurrentMessageInformation arg1, Exception arg2)
		{
			if (Logger.IsInfoEnabled)
				Logger.Info("Message processing completed, logging off from domain", arg2);
		}

		bool transport_MessageArrived(Rhino.ServiceBus.Impl.CurrentMessageInformation arg)
		{
			var logOnInfo = arg.Message as ILogOnContext;
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Message recieved. Message Id = {0}", arg.MessageId);
			}
			if (logOnInfo == null) return false;
			if (Logger.IsInfoEnabled)
			{
				Logger.InfoFormat(
					 "Message requires the domain. (Message Id = {0}, DataSource = {1}, BusinessUnit = {2}, Timestamp = {3})",
					 arg.MessageId, logOnInfo.LogOnDatasource, logOnInfo.LogOnBusinessUnitId, DateTime.UtcNow);
			}

			var dataSourceInfo = getDataSource(_dataSourceForTenant.DataSource()(), logOnInfo.LogOnDatasource, _repositoryFactory);

			if (Logger.IsInfoEnabled)
				Logger.Info("UnitOfWorkFactory configured");

			logOnRaptorDomain(dataSourceInfo.DataSource, logOnInfo.LogOnBusinessUnitId);

			if (Logger.IsInfoEnabled)
				Logger.Info("Logged on the domain");

			return false;
		}

		private static sdkDataSourceResult getDataSource(IDataSourceForTenant dataSourceForTenant, string tenant, IRepositoryFactory repositoryFactory)
		{
			var ret = new sdkDataSourceResult();
			var dataSource = dataSourceForTenant.Tenant(tenant);
			if (dataSource == null)
			{
				Logger.ErrorFormat("No datasource matching the name {0} was found.", tenant);
				throw new ArgumentNullException("tenant", string.Format(CultureInfo.InvariantCulture, "No datasource matching the name {0} was found.", tenant));
			}
			ret.DataSource = dataSource;
			return ret;
		}

		private class sdkDataSourceResult
		{
			public IDataSource DataSource { get; set; }
		}

		private void logOnRaptorDomain(IDataSource dataSource, Guid businessUnitId)
		{
			if (!DefinedLicenseDataFactory.HasLicense(dataSource.DataSourceName))
			{
				var unitOfWorkFactory = dataSource.Application;
				var licenseVerifier = new LicenseVerifier(new LicenseFeedback(), unitOfWorkFactory,
																		new LicenseRepository(unitOfWorkFactory));
				var licenseService = licenseVerifier.LoadAndVerifyLicense();
				if (licenseService == null)
				{
					Logger.Error("No license could be found.");
					return;
				}

				LicenseProvider.ProvideLicenseActivator(unitOfWorkFactory.Name, licenseService);
			}

			_asSuperUser.Logon(dataSource, businessUnitId);
			
			AuthenticationMessageHeader.BusinessUnit = businessUnitId;
			AuthenticationMessageHeader.DataSource = dataSource.Application.Name;
			AuthenticationMessageHeader.UserName = SuperUser.Id_AvoidUsing_This.ToString(); //rk - is this really correct - why the guid as username?
			AuthenticationMessageHeader.Password = "custom";
			AuthenticationMessageHeader.UseWindowsIdentity = false;

		}
		
		public void Stop(ITransport transport, IServiceBus bus)
		{
			transport.MessageArrived -= transport_MessageArrived;
			transport.MessageProcessingCompleted -= transport_MessageProcessingCompleted;
		}

		private class LicenseFeedback : ILicenseFeedback
		{
			public void Warning(string warning)
			{
				Warning(warning, "");
			}

			public void Warning(string warning, string caption)
			{
				Logger.Warn(warning);
			}

			public void Error(string error)
			{
				Logger.Error(error);
			}
		}
	}

}
