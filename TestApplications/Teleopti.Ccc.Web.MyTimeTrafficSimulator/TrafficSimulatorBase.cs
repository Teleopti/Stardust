﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Teleopti.Ccc.Web.MyTimeTrafficSimulator
{
    public abstract class TrafficSimulatorBase : IDisposable
    {
        private string _baseUrl;
        private CookieContainer _cookieContainer;
        private HttpClientHandler _httpClientHandler;
        protected HttpClient HttpClient;
	    private string _username;
	    private string _password;
	    private string _businessUnitName;

        public void Start(string baseUrl, string businessUnitName, string user, string password)
        {
            _baseUrl = baseUrl;
            _cookieContainer = new CookieContainer();
            _httpClientHandler = new HttpClientHandler { CookieContainer = _cookieContainer };
            HttpClient = new HttpClient(_httpClientHandler) { BaseAddress = new Uri(_baseUrl) };
			var defaultUser = UserData.TestUsers.First();
			_username = user ?? defaultUser.Username;
	        _password = password ?? defaultUser.Password;
	        _businessUnitName = businessUnitName ?? "Teleopti%20WFM%20Demo";
        }

	    public TimeSpan LogOn()
	    {
			
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			LogOn(_businessUnitName, _username, _password);
			stopwatch.Stop();
			return stopwatch.Elapsed;
		}

        private void LogOn(string businessUnitName, string user, string password)
        {
			var message = new HttpRequestMessage(HttpMethod.Get, string.Format("Test/Logon?businessUnitName={0}&userName={1}&password={2}&isPersistent=true", businessUnitName, user, password));
            var response = HttpClient.SendAsync(message).Result;

            if (!response.IsSuccessStatusCode)
				throw new Exception("Unable to log on");

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