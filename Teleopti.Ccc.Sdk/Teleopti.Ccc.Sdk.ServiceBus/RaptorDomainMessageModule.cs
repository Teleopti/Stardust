using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Threading;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using log4net;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.MessageModules;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
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
        private readonly IRoleToClaimSetTransformer _roleToClaimSetTransformer;
        private readonly IRepositoryFactory _repositoryFactory;
    	private readonly ClaimCache _claimCache;

    	public RaptorDomainMessageModule(ILogOnOff logOnOff, IApplicationDataSourceProvider applicationDataSourceProvider, IRoleToClaimSetTransformer roleToClaimSetTransformer, IRepositoryFactory repositoryFactory, ClaimCache claimCache)
        {
            _logOnOff = logOnOff;
            _applicationDataSourceProvider = applicationDataSourceProvider;
            _roleToClaimSetTransformer = roleToClaimSetTransformer;
            _repositoryFactory = repositoryFactory;
        	_claimCache = claimCache;
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
			var raptorDomainMessage = arg.Message as IRaptorDomainMessageInfo;
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
            var dataSource = dataSourceContainer.DataSource;
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

            using (IUnitOfWork unitOfWork = dataSource.Application.CreateAndOpenUnitOfWork())
            {
                IBusinessUnit businessUnit = _repositoryFactory.CreateBusinessUnitRepository(unitOfWork).Get(businessUnitId);
                unitOfWork.Remove(businessUnit); //To make sure that business unit doesn't belong to this uow any more

                AuthenticationMessageHeader.BusinessUnit = businessUnitId;
                AuthenticationMessageHeader.DataSource = dataSource.Application.Name;
                if (dataSourceContainer.User.ApplicationAuthenticationInfo != null)
                {
                    AuthenticationMessageHeader.UserName = dataSourceContainer.User.ApplicationAuthenticationInfo.ApplicationLogOnName;
                    AuthenticationMessageHeader.Password = SuperUser.Password;
                }
                
                AuthenticationMessageHeader.UseWindowsIdentity = false;
                _logOnOff.LogOn(dataSource, dataSourceContainer.User, businessUnit);

				setCorrectPermissionsOnUser(unitOfWork);
            }
        }

    	private void setCorrectPermissionsOnUser(IUnitOfWork unitOfWork)
    	{
    		var person = TeleoptiPrincipal.Current.GetPerson(_repositoryFactory.CreatePersonRepository(unitOfWork));
    		foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
    		{
    			var cachedClaim = _claimCache.Get(AuthenticationMessageHeader.DataSource, applicationRole.Id.GetValueOrDefault());
    			if (cachedClaim == null)
    			{
    				cachedClaim = _roleToClaimSetTransformer.Transform(applicationRole, unitOfWork);
    				_claimCache.Add(cachedClaim, AuthenticationMessageHeader.DataSource, applicationRole.Id.GetValueOrDefault());
    			}
    			TeleoptiPrincipal.Current.AddClaimSet(cachedClaim);
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

    public static class UnitOfWorkFactoryContainer
    {
        [ThreadStatic]
			private static ICurrentUnitOfWorkFactory _current;

        public static ICurrentUnitOfWorkFactory Current
        {
            get { return _current ?? UnitOfWorkFactory.CurrentUnitOfWorkFactory(); }
            set { _current = value; }
        }
    }

	public class ClaimCache : IDisposable
	{
		private readonly ConcurrentDictionary<string, ClaimSet> _cache = new ConcurrentDictionary<string,ClaimSet>();
		private readonly Timer _timer;

		public ClaimCache()
		{
			_timer = new Timer(emptyCache,null,TimeSpan.FromMilliseconds(-1),TimeSpan.FromMinutes(30));
		}

		private void emptyCache(object state)
		{
			_cache.Clear();
		}

		public void Add(ClaimSet claimSet, string dataSourceName, Guid roleId)
		{
			var key = getKey(dataSourceName, roleId);
			_cache.AddOrUpdate(key, claimSet, (s, set) => claimSet);
		}

		private static string getKey(string dataSourceName, Guid roleId)
		{
			string key = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", dataSourceName, roleId);
			return key;
		}

		public ClaimSet Get(string dataSourceName, Guid roleId)
		{
			var key = getKey(dataSourceName, roleId);
			ClaimSet foundClaimSet;
			return (_cache.TryGetValue(key, out foundClaimSet)) ? foundClaimSet : null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_timer != null)
				{
					_timer.Dispose();
				}
				_cache.Clear();
			}
		}
	}
}
