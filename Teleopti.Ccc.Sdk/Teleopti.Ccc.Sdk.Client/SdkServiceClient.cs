using System;
using System.Net;
using System.Web.Services.Protocols;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.Sdk.Client
{
	public interface ISessionStateProvider
	{
		bool IsLoggedIn { get; }
		PersonDto LoggedOnPerson { get; }
		BusinessUnitDto BusinessUnit { get; }
		DataSourceDto DataSource { get; }
		string Password { get; }
	}

	public class SdkServiceClient : IDisposable
	{
		private TeleoptiCccSdkService _teleoptiSdkService;
		private TeleoptiCccLogOnService _logOnServiceClient;
		private TeleoptiSchedulingService _schedulingService;
		private TeleoptiOrganizationService _organizationService;
		private TeleoptiOrganizationService1 _internalService;

		private readonly ISessionStateProvider _sessionStateProvider;
		private readonly string _serviceUrl;

		public SdkServiceClient(ISessionStateProvider sessionStateProvider, string serviceUrl)
		{
			_sessionStateProvider = sessionStateProvider;
			_serviceUrl = serviceUrl;
		}

		private bool IsInitialized { get { return _logOnServiceClient != null; } }

		private void Initialize()
		{
			_logOnServiceClient = new TeleoptiCccLogOnService();
			InitializeService(_logOnServiceClient);
		}

		private void InitializeIfNeeded()
		{
			if (!IsInitialized)
				Initialize();
		}

		private void CheckState()
		{
			InitializeIfNeeded();
			if (_sessionStateProvider.IsLoggedIn)
			{
                /*
				bool isAuthenticated;
				bool isAuthenticatedSpecified;
				_logOnServiceClient.IsAuthenticated(out isAuthenticated, out isAuthenticatedSpecified);

				if (!isAuthenticated)
				{
					_logOnServiceClient.TransferSession(MakeSessionDataDto());
					_schedulingService = null;
					_organizationService = null;
					_teleoptiSdkService = null;
				}*/
			}
		}
        /*
		private SessionDataDto MakeSessionDataDto()
		{
			var sessionDataDto = new SessionDataDto();
			sessionDataDto.LoggedOnPerson = _sessionStateProvider.LoggedOnPerson;
			sessionDataDto.BusinessUnit = _sessionStateProvider.BusinessUnit;
			sessionDataDto.DataSource = _sessionStateProvider.DataSource;
			sessionDataDto.AuthenticationType = sessionDataDto.DataSource.AuthenticationTypeOptionDto;
			sessionDataDto.AuthenticationTypeSpecified = true;
			sessionDataDto.LoggedOnPassword = _sessionStateProvider.Password;
			return sessionDataDto;
		}*/

		public TeleoptiCccLogOnService LogOnServiceClient
		{
			get
			{
				CheckState();
                /*
				if (_sessionStateProvider.IsLoggedIn)
				{
					bool isAuthenticated, isAuthenticatedSpecified;
					_logOnServiceClient.IsAuthenticated(out isAuthenticated, out isAuthenticatedSpecified);
					if (!isAuthenticated)
					{
						_logOnServiceClient.TransferSession(MakeSessionDataDto());
					}
				}*/

				return _logOnServiceClient;
			}
		}

		public TeleoptiSchedulingService SchedulingService
		{
			get
			{
				CheckState();
				if (_schedulingService == null)
				{
					_schedulingService = new TeleoptiSchedulingService();
					InitializeService(_schedulingService);
				}
				return _schedulingService;
			}
		}

		public TeleoptiOrganizationService OrganizationService
		{
			get
			{
				CheckState();
				if (_organizationService == null)
				{
					_organizationService = new TeleoptiOrganizationService();
					InitializeService(_organizationService);
				}
				return _organizationService;
			}
		}

		public TeleoptiCccSdkService TeleoptiSdkService
		{
			get
			{
				CheckState();
				if (_teleoptiSdkService == null)
				{
					_teleoptiSdkService = new TeleoptiCccSdkService();
					InitializeService(_teleoptiSdkService);
				}
				return _teleoptiSdkService;
			}
		}

        public TeleoptiOrganizationService1 TeleoptiInternalService
        {
            get
            {
                CheckState();
                if (_internalService == null)
                {
                    _internalService = new TeleoptiOrganizationService1();
                    InitializeService(_internalService);
                }
                return _internalService;
            }
        }

		private void InitializeService(HttpWebClientProtocol service)
		{
			var cache = new CredentialCache
			            {
			            	{new Uri(_serviceUrl), "NTLM", CredentialCache.DefaultNetworkCredentials},
			            	{new Uri(_serviceUrl), "Kerberos", CredentialCache.DefaultNetworkCredentials}
			            };
			//service.CookieContainer = _cookieContainer;
			service.Credentials = cache;
			service.Url = _serviceUrl;
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
			if (_teleoptiSdkService != null)
				_teleoptiSdkService.Dispose();
			if (_logOnServiceClient != null)
				_logOnServiceClient.Dispose();
			if (_schedulingService != null)
				_schedulingService.Dispose();
			if (_organizationService != null)
				_organizationService.Dispose();
            if (_internalService != null)
                _internalService.Dispose();
		}

	}
}