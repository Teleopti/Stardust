using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security;
using log4net;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.MessageModules;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class ApplicationLogOnMessageModule : IMessageModule
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ApplicationLogOnMessageModule));
		private readonly DataSourceForTenantWrapper _dataSourceForTenant;
		private readonly Lazy<AsSystem> _asSystem;

		public ApplicationLogOnMessageModule(
			DataSourceForTenantWrapper dataSourceForTenant,
			Lazy<AsSystem> asSystem)
		{
			_dataSourceForTenant = dataSourceForTenant;
			_asSystem = asSystem;
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

			if (Logger.IsInfoEnabled)
				Logger.Info("UnitOfWorkFactory configured");

			if (checkLicense(logOnInfo.LogOnDatasource))
			{
				_asSystem.Value.Logon(logOnInfo.LogOnDatasource, logOnInfo.LogOnBusinessUnitId);
				setWcfAuthenticationHeader(logOnInfo.LogOnDatasource, logOnInfo.LogOnBusinessUnitId);
			}

			if (Logger.IsInfoEnabled)
				Logger.Info("Logged on the domain");

			return false;
		}

		private bool checkLicense(string tenant)
		{
			var dataSource = _dataSourceForTenant.DataSource()().Tenant(tenant);
			if (dataSource == null)
			{
				Logger.ErrorFormat("No datasource matching the name {0} was found.", tenant);
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "No datasource matching the name {0} was found.", tenant));
			}

			if (DefinedLicenseDataFactory.HasLicense(tenant))
				return true;

			var licenseVerifier = new LicenseVerifier(
				new logLicenseFeedback(),
				dataSource.Application,
				new LicenseRepository(new FromFactory(() => dataSource.Application))
				);
			var licenseService = licenseVerifier.LoadAndVerifyLicense();
			if (licenseService == null)
			{
				Logger.Error("No license could be found.");
				return false;
			}

			LicenseProvider.ProvideLicenseActivator(dataSource.DataSourceName, licenseService);
			return true;
		}

		private static void setWcfAuthenticationHeader(string dataSourceName, Guid businessUnitId)
		{
			AuthenticationMessageHeader.BusinessUnit = businessUnitId;
			AuthenticationMessageHeader.DataSource = dataSourceName;
			AuthenticationMessageHeader.UserName = SystemUser.Id.ToString(); //rk - is this really correct - why the guid as username?
			AuthenticationMessageHeader.Password = "custom";
			AuthenticationMessageHeader.UseWindowsIdentity = false;
		}

		public void Stop(ITransport transport, IServiceBus bus)
		{
			transport.MessageArrived -= transport_MessageArrived;
			transport.MessageProcessingCompleted -= transport_MessageProcessingCompleted;
		}

		private class logLicenseFeedback : ILicenseFeedback
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
