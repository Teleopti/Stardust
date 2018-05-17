using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Web.MyTimeTrafficSimulator.ListenSimulators
{
	public abstract class SimulateBase<T>
	{
		private string _number;
		private readonly IMessageBrokerUrl _url;
		protected readonly ICurrentDataSource DataSource;
        protected readonly ICurrentBusinessUnit BusinessUnit;
        protected readonly ICurrentScenario Scenario;
		private readonly IJsonSerializer _serializer;
		private readonly IMessageBrokerComposite _messageBroker;
		protected readonly HttpClient HttpClient;


		protected string BaseUrl;
		private readonly CookieContainer cookieContainer;
		private HttpClientHandler httpClientHandler;

	    protected SimulateBase(
			IMessageBrokerUrl url,
			ICurrentDataSource dataSource,
			ICurrentBusinessUnit businessUnit,
			ICurrentScenario scenario,
			IJsonSerializer serializer,
			IMessageBrokerComposite messageBroker)
		{
			_url = url;
			DataSource = dataSource;
			BusinessUnit = businessUnit;
			Scenario = scenario;
			_serializer = serializer;
			_messageBroker = messageBroker;

			BaseUrl = url.Url;
			cookieContainer = new CookieContainer();
			httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer };
			HttpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri(BaseUrl), Timeout = TimeSpan.FromSeconds(600)};
		}

		public void LogOn(string businessUnitName, string user, string password)
		{
			var message = new HttpRequestMessage(HttpMethod.Get, string.Format("Test/Logon?businessUnitName={0}&userName={1}&password={2}&isPersistent=true", businessUnitName, user, password));
			var response = HttpClient.SendAsync(message).GetAwaiter().GetResult();

			if (!response.IsSuccessStatusCode) throw new Exception("Unable to log on");
			var headers = response.Headers;
			foreach (var cookie in headers.GetValues("Set-Cookie").Select(item => item.Split(';').First().Split('=')))
			{
				cookieContainer.Add(new Uri(BaseUrl), new Cookie(cookie.First(), cookie.Last()));
			}
			HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		private string url(string call)
		{
			if (_url == null)
				return null;
			if (string.IsNullOrEmpty(_url.Url))
				return null;
			return _url.Url.TrimEnd('/') + "/" + call;
		}

	    public void Simulate(T data, int screen, int client, Action callback)
	    {
	        _number = string.Format("#{0}/{1}", screen, client);
            //Console.WriteLine("Starting screen {0}/{1}", screen, client);
            Simulate(data, callback);
	    }
        public abstract void Simulate(T data, Action callback);

        private void addMailbox(Subscription subscription)
		{
			var content = _serializer.SerializeObject(subscription);
			post(url("MessageBroker/AddMailbox"), content);
			Console.WriteLine(_number + "mailbox created " + subscription.MailboxId);
			//startPoll(subscription.MailboxId);
		}

		private async void startPoll(string mailboxId)
		{
			while (true)
			{
				await Task.Delay(TimeSpan.FromSeconds(5));
				Console.WriteLine(_number + " polling " + mailboxId + " got " + await get(url("MessageBroker/PopMessages?id=" + mailboxId)));
			}
		}

		private async void post(string url, string content)
		{
			var result = await HttpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception("POST failed! " + result.StatusCode);
		}

		private async Task<string> get(string url)
		{
			var result = await HttpClient.GetAsync(url);
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception("GET failed! " + result.StatusCode);
			return await result.Content.ReadAsStringAsync();
		}

		protected void AddSubscription(Subscription subscription, EventHandler<EventMessageArgs> callback)
		{
			_messageBroker.RegisterSubscription(subscription, callback);
		}

		private void callback(object sender, EventMessageArgs e)
		{
            Console.WriteLine(_number + " callbacked");
        }

		public abstract void CallbackAction();
		public abstract void LogOn(T data);
	}


}