using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IAggregatedScheduleChangeSender
	{
		void Send(List<AggregatedScheduleChangedInfo> modified, IScenario scenario);
	}
	public class AggregatedScheduleChangeSender : IAggregatedScheduleChangeSender
	{
		private readonly IMessageSender _messageSender;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly IJsonSerializer _serializer;
		private readonly IInitiatorIdentifier _initiatorIdentifier;

		public AggregatedScheduleChangeSender(
			IMessageSender messageSender,
			ICurrentDataSource dataSource,
			ICurrentBusinessUnit businessUnit,
			IJsonSerializer serializer,
			IInitiatorIdentifier initiatorIdentifier)
		{
			_messageSender = messageSender;
			_dataSource = dataSource;
			_businessUnit = businessUnit;
			_serializer = serializer;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public void Send(List<AggregatedScheduleChangedInfo> modified, IScenario scenario)
		{
			var personIds = new HashSet<Guid>();
			var startDate = DateTime.MaxValue;
			var endDate = DateTime.MinValue;
			foreach (var pa in modified)
			{
				personIds.Add(pa.PersonId);
				startDate = new[] { pa.Period.StartDateTime, startDate }.Min();
				endDate = new[] { pa.Period.EndDateTime, endDate }.Max();
			}

			_messageSender.Send(new Message
			{
				ModuleId = _initiatorIdentifier.InitiatorId.ToString(),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
				StartDate = Subscription.DateToString(startDate),
				EndDate = Subscription.DateToString(endDate),
				DomainReferenceId = scenario.Id.Value.ToString(),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IAggregatedScheduleChange).AssemblyQualifiedName,
				DomainType = typeof(IAggregatedScheduleChange).Name,
				BinaryData = _serializer.SerializeObject(personIds)
			});
		}
	}
}