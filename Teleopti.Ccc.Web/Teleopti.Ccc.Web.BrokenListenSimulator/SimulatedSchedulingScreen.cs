using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Ccc.Web.BrokenListenSimulator
{
	public class SimulatedSchedulingScreen
	{
		private readonly IMessageBrokerUrl _url;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly ICurrentScenario _scenario;
		private readonly IJsonSerializer _serializer;
		private readonly HttpClient _httpClient;

		public SimulatedSchedulingScreen(
			IMessageBrokerUrl url,
			ICurrentDataSource dataSource, 
			ICurrentBusinessUnit businessUnit, 
			ICurrentScenario scenario,
			IJsonSerializer serializer)
		{
			_url = url;
			_dataSource = dataSource;
			_businessUnit = businessUnit;
			_scenario = scenario;
			_serializer = serializer;
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

		public void Simulate(int number, string brokerUrl, DateTime startDate, DateTime endDate)
		{
			_url.Configure(brokerUrl);

			addMailbox(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IScheduleChangedEvent).Name,
				DomainReferenceId = Subscription.IdToString(_scenario.Current().Id.Value) ,
				DomainReferenceType = typeof(Scenario).AssemblyQualifiedName,
				LowerBoundary = Subscription.DateToString(startDate),
				UpperBoundary = Subscription.DateToString(endDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			});

			addMailbox(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof (IPersistableScheduleData).Name,
				DomainReferenceId = null,
				DomainReferenceType = null,
				LowerBoundary = Subscription.DateToString(startDate),
				UpperBoundary = Subscription.DateToString(endDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			});

			addMailbox(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IMeeting).Name,
				DomainReferenceId = null,
				DomainReferenceType = null,
				LowerBoundary = Subscription.DateToString(Consts.MinDate),
				UpperBoundary = Subscription.DateToString(Consts.MaxDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			});

			addMailbox(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IPersonRequest).Name,
				DomainReferenceId = null,
				DomainReferenceType = null,
				LowerBoundary = Subscription.DateToString(Consts.MinDate),
				UpperBoundary = Subscription.DateToString(Consts.MaxDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			});

		}

		private void addMailbox(Subscription subscription)
		{
			var content = _serializer.SerializeObject(subscription);
			post(url("MessageBroker/AddMailbox"), content);
			startPoll(subscription.MailboxId);
		}

		private void startPoll(string mailboxId)
		{
			return;
			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					Task.Delay(TimeSpan.FromSeconds(30))
						.ContinueWith(t => Console.WriteLine(get(url("MessageBroker/PopMessages?id=" + mailboxId))))
						.Wait();
				}
			});
		}

		private async void post(string url, string content)
		{
			var x = new Stopwatch();
			x.Start();
			var result = await _httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception("POST failed! " + result.StatusCode);
			x.Stop();
			Console.WriteLine("mailbox created and it took: " + x.ElapsedMilliseconds);
		}

		private async Task<string> get(string url)
		{
			var result = await _httpClient.GetAsync(url);
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception("GET failed! " + result.StatusCode);
			return await result.Content.ReadAsStringAsync();
		}
	}


}