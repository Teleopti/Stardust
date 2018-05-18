using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.BrokerListenSimulator
{

	public class SimulateSchedulingScreen 
    {
	    private readonly IMessageBrokerUrl _url;
	    private readonly ICurrentDataSource _dataSource;
	    private readonly ICurrentBusinessUnit _businessUnit;
	    private readonly ICurrentScenario _scenario;
	    private readonly IJsonSerializer _serializer;
	    private readonly IJsonDeserializer _deserializer;
	    private readonly IMessageBrokerComposite _messageBroker;
	    private readonly HttpClient _httpClient;
	    private string _number;

	    public SimulateSchedulingScreen(
			IMessageBrokerUrl url, 
			ICurrentDataSource dataSource, 
			ICurrentBusinessUnit businessUnit, 
			ICurrentScenario scenario, 
			IJsonSerializer serializer, 
			IJsonDeserializer deserializer,
			IMessageBrokerComposite messageBroker)
	    {
		    _url = url;
		    _dataSource = dataSource;
		    _businessUnit = businessUnit;
		    _scenario = scenario;
		    _serializer = serializer;
		    _deserializer = deserializer;
		    _messageBroker = messageBroker;
		    _httpClient = new HttpClient();
		}

		public void Simulate(SimulationData data, int screen, int client)
		{
			_number = $"#{screen}/{client}";

				//AddSubscription(new Subscription
				//{
				//	DomainType = typeof(IScheduleChangedEvent).Name,
				//	DomainReferenceId = Subscription.IdToString(_scenario.Current().Id.Value),
				//	DomainReferenceType = typeof(Scenario).AssemblyQualifiedName,
				//	LowerBoundary = Subscription.DateToString(data.SchedulingScreenStartDate),
				//	UpperBoundary = Subscription.DateToString(data.SchedulingScreenEndDate),
				//	DataSource = _dataSource.CurrentName(),
				//	BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
				//}, callback);

			AddSubscription(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IScheduleChangedMessage).Name,
				DomainReferenceId = Subscription.IdToString(_scenario.Current().Id.Value),
				DomainReferenceType = typeof(Scenario).AssemblyQualifiedName,
				LowerBoundary = Subscription.DateToString(data.SchedulingScreenStartDate),
				UpperBoundary = Subscription.DateToString(data.SchedulingScreenEndDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			}, callback);

			AddSubscription(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IPersistableScheduleData).Name,
				DomainReferenceId = null,
				DomainReferenceType = null,
				LowerBoundary = Subscription.DateToString(data.SchedulingScreenStartDate),
				UpperBoundary = Subscription.DateToString(data.SchedulingScreenEndDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			}, callback);

			AddSubscription(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IMeeting).Name,
				DomainReferenceId = null,
				DomainReferenceType = null,
				LowerBoundary = Subscription.DateToString(Consts.MinDate),
				UpperBoundary = Subscription.DateToString(Consts.MaxDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			}, callback);

			AddSubscription(new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IPersonRequest).Name,
				DomainReferenceId = null,
				DomainReferenceType = null,
				LowerBoundary = Subscription.DateToString(Consts.MinDate),
				UpperBoundary = Subscription.DateToString(Consts.MaxDate),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
			}, callback);
		}
		
	    private void AddSubscription(Subscription subscription, EventHandler<EventMessageArgs> callback)
		{
			//_messageBroker.RegisterSubscription(subscription, callback);
			startPoll(subscription);
		}

		private string url(string call)
		{
			if (_url == null)
				return null;
			if (string.IsNullOrEmpty(_url.Url))
				return null;
			return $"{_url.Url.TrimEnd('/')}/{call}";
		}

		private void startPoll(Subscription subscription)
		{
			get(url($"MessageBroker/PopMessages?route={subscription.Route()}&id={subscription.MailboxId}"));

			//Task.Factory.StartNew(() =>
			//{
			//	while (true)
			//	{
			//		Task.Delay(TimeSpan.FromSeconds(120)).Wait(); // default at this time
			//		var result = get(url($"MessageBroker/PopMessages?route={subscription.Route()}&id={subscription.MailboxId}"));
			//		var messages = _deserializer.DeserializeObject<Message[]>(result);
			//		messages.ForEach(m =>
			//		{
			//			callback(this, null);
			//		});
			//	}
			//}, TaskCreationOptions.LongRunning);
		}

		private void post(string url, string content)
		{
			var result = _httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json")).Result;
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception($"POST failed! {result.StatusCode}");
		}

		private string get(string url)
		{
			var result = _httpClient.GetAsync(url).Result;
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception($"GET failed! {result.StatusCode}");
			return result.Content.ReadAsStringAsync().Result;
		}

		private void callback(object sender, EventMessageArgs e)
		{
			Console.WriteLine($"{_number} callbacked");
		}
	}

}