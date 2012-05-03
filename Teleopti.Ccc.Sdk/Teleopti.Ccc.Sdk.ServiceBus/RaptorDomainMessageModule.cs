using System;
using log4net;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.MessageModules;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Obfuscated.Security;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class RaptorDomainMessageModule : IMessageModule
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (RaptorDomainMessageModule));
        private readonly ILogOnOff _logOnOff;
        private readonly IApplicationDataSourceProvider _applicationDataSourceProvider;
        private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
        private readonly IRepositoryFactory _repositoryFactory;

        public RaptorDomainMessageModule(ILogOnOff logOnOff, IApplicationDataSourceProvider applicationDataSourceProvider, IRoleToPrincipalCommand roleToPrincipalCommand, IRepositoryFactory repositoryFactory)
        {
            _logOnOff = logOnOff;
            _applicationDataSourceProvider = applicationDataSourceProvider;
            _roleToPrincipalCommand = roleToPrincipalCommand;
            _repositoryFactory = repositoryFactory;
        }

        public void Init(ITransport transport, IServiceBus bus)
        {
            transport.MessageArrived += transport_MessageArrived;
            transport.MessageProcessingCompleted += transport_MessageProcessingCompleted;
        }

        void transport_MessageProcessingCompleted(Rhino.ServiceBus.Impl.CurrentMessageInformation arg1, Exception arg2)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info("Message processing completed, logging off from domain",arg2);

            _logOnOff.LogOff();
        }

        bool transport_MessageArrived(Rhino.ServiceBus.Impl.CurrentMessageInformation arg)
        {
            RaptorDomainMessage raptorDomainMessage = arg.Message as RaptorDomainMessage;
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Message recieved. Message Id = {0}", arg.MessageId);
            }
            if (raptorDomainMessage == null) return false;
            if (Logger.IsInfoEnabled)
            {
                Logger.InfoFormat(
                    "Message requires the domain. (Message Id = {0}, DataSource = {1}, BusinessUnit = {2}, Timestamp = {3})",
                    arg.MessageId, raptorDomainMessage.Datasource, raptorDomainMessage.BusinessUnitId,
                    raptorDomainMessage.Timestamp);
            }

            var dataSourceContainer = DataSourceFactory.GetDesiredDataSource(_applicationDataSourceProvider,
                                                                           raptorDomainMessage.Datasource);
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("UnitOfWorkFactory configured");
            }
            LogOnRaptorDomain(dataSourceContainer, raptorDomainMessage.BusinessUnitId);
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Logged on the domain");
            }

            return false;
        }

        private void LogOnRaptorDomain(DataSourceContainer dataSourceContainer, Guid businessUnitId)
        {
            if (DefinedLicenseDataFactory.LicenseActivator == null)
            {
                var unitOfWorkFactory = dataSourceContainer.DataSource.Application;
            	var licenseVerifier = new LicenseVerifier(new LicenseFeedback(), unitOfWorkFactory,
            	                                          new LicenseRepository(unitOfWorkFactory));
                var licenseService = licenseVerifier.LoadAndVerifyLicense();
                if (licenseService == null)
                {
                    Logger.Error("No license could be found.");
                    return;
                }

                LicenseProvider.ProvideLicenseActivator(licenseService);
            }

            using (IUnitOfWork unitOfWork = dataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                IBusinessUnit businessUnit = _repositoryFactory.CreateBusinessUnitRepository(unitOfWork).Get(businessUnitId);
                unitOfWork.Remove(businessUnit); //To make sure that business unit doesn't belong to this uow any more

                AuthenticationMessageHeader.BusinessUnit = businessUnitId;
                AuthenticationMessageHeader.DataSource = dataSourceContainer.DataSource.Application.Name;
                if (dataSourceContainer.User.ApplicationAuthenticationInfo != null)
                {
                    AuthenticationMessageHeader.UserName = dataSourceContainer.User.ApplicationAuthenticationInfo.ApplicationLogOnName;
                    AuthenticationMessageHeader.Password = SuperUser.Password;
                }
                
                AuthenticationMessageHeader.UseWindowsIdentity = false;
                _logOnOff.LogOn(dataSourceContainer.DataSource, dataSourceContainer.User, businessUnit, AuthenticationTypeOption.Application);

				_roleToPrincipalCommand.Execute(TeleoptiPrincipal.Current, unitOfWork, _repositoryFactory.CreatePersonRepository(unitOfWork));
            }
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
				Logger.Warn(warning);
			}

			public void Error(string error)
			{
				Logger.Error(error);
			}
		}
    }

    public static class UnitOfWorkFactoryContainer
    {
        [ThreadStatic]
        private static IUnitOfWorkFactory _current;

        public static IUnitOfWorkFactory Current
        {
            get { return _current ?? ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).DataSource.Application; }
            set { _current = value; }
        }
    }
}
