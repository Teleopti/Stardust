using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.TrafficSimulators
{
    public abstract class TrafficSimulatorBase<T> : IDisposable where T: SimulationDataBase
    {
        private string _baseUrl;
        private CookieContainer cookieContainer;
        private HttpClientHandler httpClientHandler;
        protected HttpClient HttpClient;

        public void Start(string baseUrl, string businessUnitName, string user, string password)
        {
            _baseUrl = baseUrl;
            cookieContainer = new CookieContainer();
            httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer };
            HttpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri(_baseUrl) };
            logOn(businessUnitName ?? "Teleopti%20WFM%20Demo", user ?? "demo", password ?? "demo");
        }

        private void logOn(string businessUnitName, string user, string password)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, string.Format("Test/Logon?businessUnitName={0}&userName={1}&password={2}", businessUnitName, user, password));
            var response = HttpClient.SendAsync(message).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode) throw new Exception("Unable to log on");
            var headers = response.Headers;
            foreach (var cookie in headers.GetValues("Set-Cookie").Select(item => item.Split(';').First().Split('=')))
            {
                cookieContainer.Add(new Uri(_baseUrl), new Cookie(cookie.First(), cookie.Last()));
            }
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public abstract void AddFullDayAbsenceForThePersonByNextNDays(T data, int days);
		public abstract void AddFullDayAbsenceForAllPeopleWithPartTimePercentage100ByNextNDays(T data, int days);
        

        public void Dispose()
        {
            httpClientHandler.Dispose();
            HttpClient.Dispose();
        }

        public abstract void CallbackAction();
    }
}