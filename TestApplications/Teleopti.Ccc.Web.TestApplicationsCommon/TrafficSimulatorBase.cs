using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Teleopti.Ccc.Web.TestApplicationsCommon
{
    public abstract class TrafficSimulatorBase : IDisposable
    {
        private string _baseUrl;
        private CookieContainer _cookieContainer;
        private HttpClientHandler _httpClientHandler;
        protected HttpClient HttpClient;

        public void Start(string baseUrl, string businessUnitName, string user, string password)
        {
            _baseUrl = baseUrl;
            _cookieContainer = new CookieContainer();
            _httpClientHandler = new HttpClientHandler { CookieContainer = _cookieContainer };
            HttpClient = new HttpClient(_httpClientHandler) { BaseAddress = new Uri(_baseUrl) };
	        var defaultUser = UserData.TestUsers.First();
			LogOn(businessUnitName ?? "Teleopti%20WFM%20Demo", user ?? defaultUser.Username, password ?? defaultUser.Password);
        }

        private void LogOn(string businessUnitName, string user, string password)
        {
			var message = new HttpRequestMessage(HttpMethod.Get, string.Format("Test/Logon?businessUnitName={0}&userName={1}&password={2}&isPersistent=true", businessUnitName, user, password));
            var response = HttpClient.SendAsync(message).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode) throw new Exception("Unable to log on");
            var headers = response.Headers;
            foreach (var cookie in headers.GetValues("Set-Cookie").Select(item => item.Split(';').First().Split('=')))
            {
                _cookieContainer.Add(new Uri(_baseUrl), new Cookie(cookie.First(), cookie.Last()));
            }
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

		public void Dispose()
        {
            _httpClientHandler.Dispose();
            HttpClient.Dispose();
        }
    }
}