using System;
using System.Net;
using System.Net.Http;
using System.Text;
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
		private string _number;
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
			_number = "#" + number;
			_url.Configure(brokerUrl);

			addMailbox(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IScheduleChangedMessage).Name,
				DomainReferenceId = Subscription.IdToString(_scenario.Current().Id.Value),
				DomainReferenceType = typeof(Scenario).AssemblyQualifiedName,
				LowerBoundary = Subscription.DateToString(startDate),
				UpperBoundary = Subscription.DateToString(endDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			});

			addMailbox(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IPersistableScheduleData).Name,
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
	}


}