using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.ListenSimulators
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
		private readonly HttpClient _httpClient;

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
			_httpClient = new HttpClient();
		}

		private string url(string call)
		{
			if (_url == null)
				return null;
			if (string.IsNullOrEmpty(_url.Url))
				return null;
			return _url.Url.TrimEnd('/') + "/" + call;
		}

	    public void Simulate(T data, int screen, int client, EventHandler<EventMessageArgs> callback)
	    {
	        _number = string.Format("#{0}/{1}", screen, client);
            //Console.WriteLine("Starting screen {0}/{1}", screen, client);
            Simulate(data, callback);
	    }
        public abstract void Simulate(T data, EventHandler<EventMessageArgs> callback);

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
			var result = await _httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception("POST failed! " + result.StatusCode);
		}

		private async Task<string> get(string url)
		{
			var result = await _httpClient.GetAsync(url);
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
	}


}