using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Ccc.Web.BrokenListenSimulator
{
	public class SchedulingScreen
	{
		private readonly IMessageBrokerUrl _url;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly ICurrentScenario _scenario;
		private readonly IJsonSerializer _serializer;

		public SchedulingScreen(
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
		}

		private string url(string call)
		{
			if (_url == null)
				return null;
			if (string.IsNullOrEmpty(_url.Url))
				return null;
			return _url.Url.TrimEnd('/') + "/" + call;
		}

		public void Simulate(string brokerUrl, DateTime startDate, DateTime endDate)
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
			var httpClient = new HttpClient();
			var content = _serializer.SerializeObject(subscription);
			var task = httpClient.PostAsync(url("MessageBroker/AddMailbox"), new StringContent(content, Encoding.UTF8, "application/json"));
			if (task.Result.StatusCode != HttpStatusCode.OK)
				throw new Exception("fail! " + task.Result.StatusCode);
		}

	}


}