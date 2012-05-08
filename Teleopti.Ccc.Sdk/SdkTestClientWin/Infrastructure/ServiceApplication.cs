using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Services.Protocols;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Infrastructure
{
    public class ServiceApplication
    {
        private  TeleoptiCccSdkService _sdkService;
        private TeleoptiForecastingService _forecastingService;
        private TeleoptiOrganizationService _organizationService;
        private TeleoptiSchedulingService _schedulingService;
        private TeleoptiCccLogOnService _logonService;
        private TeleoptiOrganizationService1 _internalService;

        private  BusinessUnitDto _businessUnit;

        public ServiceApplication(string logOnName, string password)
        {
            logon(logOnName, password);
        }

        public TeleoptiCccSdkService SdkService
        {
            get { return _sdkService; }
        }

        public BusinessUnitDto BusinessUnit
        {
            get { return _businessUnit; }
        }

        public TeleoptiForecastingService ForecastingService
        {
            get { return _forecastingService; }
        }

        public TeleoptiOrganizationService OrganizationService
        {
            get { return _organizationService; }
        }

        public TeleoptiSchedulingService SchedulingService
        {
            get { return _schedulingService; }
        }

        public TeleoptiCccLogOnService LogonService
        {
            get { return _logonService; }
        }

        public TeleoptiOrganizationService1 InternalService
        {
            get { return _internalService; }
        }

        private void logon(string logonName, string passWord)
        {
            //Create the Service object
            _sdkService = new TeleoptiCccSdkService();
            _forecastingService = new TeleoptiForecastingService();
            _schedulingService = new TeleoptiSchedulingService();
            _organizationService = new TeleoptiOrganizationService();
            _logonService = new TeleoptiCccLogOnService();
            _internalService = new TeleoptiOrganizationService1();

            addStuff(SdkService);
            addStuff(ForecastingService);
            addStuff(SchedulingService);
            addStuff(OrganizationService);
            addStuff(LogonService);
            addStuff(InternalService);

            //Get the datasources
            ICollection<DataSourceDto> availableDataSources = LogonService.GetDataSources();
            //Select the first one (generally choosen by a user)
            DataSourceDto dataSource = availableDataSources.FirstOrDefault();
            //Login to that datasource, and get the available business units

            var currentHeader = AuthenticationSoapHeader.Current;
            currentHeader.DataSource = dataSource.Name;

            var authenticationResult = LogonService.LogOnApplicationUser(logonName, passWord, dataSource);
            if (authenticationResult.Successful)
            {
                //Select the first one (generally choosen by a user)
                _businessUnit = authenticationResult.BusinessUnitCollection.FirstOrDefault(b => b.Name[0]!='A');

                currentHeader.UserName = logonName;
                currentHeader.Password = passWord;
                currentHeader.BusinessUnit = _businessUnit.Id;

                //Set the business unit
                LogonService.SetBusinessUnit(BusinessUnit);
            }
        }

        private static void addStuff(HttpWebClientProtocol ret)
        {
            var cache = new CredentialCache
                            {
                                {new Uri(ret.Url), "NTLM", CredentialCache.DefaultNetworkCredentials},
                                {new Uri(ret.Url), "Kerberos", CredentialCache.DefaultNetworkCredentials}
                            };
            ret.Credentials = cache;
        }
    }
}
