using System;
using System.Net;
using System.Net.Http;
using System.Text;
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

			AddSubscription(new Subscription
            {
				MailboxId = Guid.NewGuid().ToString(),
				DomainType = typeof(IScheduleChangedEvent).Name,
                DomainReferenceId = Subscription.IdToString(_scenario.Current().Id.Value),
                DomainReferenceType = typeof(Scenario).AssemblyQualifiedName,
                LowerBoundary = Subscription.DateToString(data.SchedulingScreenStartDate),
                UpperBoundary = Subscription.DateToString(data.SchedulingScreenEndDate),
                DataSource = _dataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(_scenario.Current().Id.Value),
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

		private async void startPoll(Subscription subscription)
		{
			while (true)
			{
				await Task.Delay(TimeSpan.FromSeconds(120)); // default at this time
				var result = await get(url($"MessageBroker/PopMessages?route={subscription.Route()}&id={subscription.MailboxId}"));
				var messages = _deserializer.DeserializeObject<Message[]>(result);
				messages.ForEach(m =>
				{
					callback(this, null);
				});
			}
		}

		private async void post(string url, string content)
		{
			var result = await _httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception($"POST failed! {result.StatusCode}");
		}

		private async Task<string> get(string url)
		{
			var result = await _httpClient.GetAsync(url);
			if (result.StatusCode != HttpStatusCode.OK)
				throw new Exception($"GET failed! {result.StatusCode}");
			return await result.Content.ReadAsStringAsync();
		}

		private void callback(object sender, EventMessageArgs e)
		{
			Console.WriteLine($"{_number} callbacked");
		}
	}

}