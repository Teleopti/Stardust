using System;
using System.Net;
using System.Web;
using System.Web.Services.Protocols;
using SdkTestClientWeb.Sdk;

namespace SdkTestClientWeb
{
    public static class ServiceFactory
    {
        public static TeleoptiSchedulingService SdkService()
        {
            TeleoptiSchedulingService ret = new TeleoptiSchedulingService();
            var cache = new CredentialCache();
            cache.Add(new Uri(ret.Url), "NTLM", CredentialCache.DefaultNetworkCredentials);
            cache.Add(new Uri(ret.Url), "Kerberos", CredentialCache.DefaultNetworkCredentials);
            ret.CookieContainer = (CookieContainer)HttpContext.Current.Session["CookieContainer"];
            ret.Credentials = cache;
            return ret;
        }

        public static TeleoptiForecastingService Forecaster()
        {
            TeleoptiForecastingService ret = new TeleoptiForecastingService();
            addStuff(ret);
            return ret;
        }

        public static TeleoptiOrganizationService Organisation()
        {
            TeleoptiOrganizationService ret = new TeleoptiOrganizationService();
            addStuff(ret);
            return ret;
        }

        public static TeleoptiCccLogOnService LogonService()
        {
            TeleoptiCccLogOnService ret = new TeleoptiCccLogOnService();
            addStuff(ret);
            return ret;
        }

        private static void addStuff(HttpWebClientProtocol ret)
        {
            var cache = new CredentialCache
                            {
                                {new Uri(ret.Url), "NTLM", CredentialCache.DefaultNetworkCredentials},
                                {new Uri(ret.Url), "Kerberos", CredentialCache.DefaultNetworkCredentials}
                            };
            ret.CookieContainer = (CookieContainer)HttpContext.Current.Session["CookieContainer"];
            ret.Credentials = cache;
        }

    }
}
