using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class AggregatedScheduleChangeSender : IMessageSender
	{
		private readonly Interfaces.MessageBroker.Client.IMessageSender _messageSender;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly ICurrentScenario _scenario;
		private readonly IJsonSerializer _serializer;

		public AggregatedScheduleChangeSender(
			Interfaces.MessageBroker.Client.IMessageSender messageSender,
			ICurrentDataSource dataSource,
			ICurrentBusinessUnit businessUnit,
			ICurrentScenario scenario,
			IJsonSerializer serializer
			)
		{
			_messageSender = messageSender;
			_dataSource = dataSource;
			_businessUnit = businessUnit;
			_scenario = scenario;
			_serializer = serializer;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var personAssignments = modifiedRoots.Select(x => x.Root).OfType<PersonAssignment>();
			if (personAssignments.IsEmpty())
				return;

			var personIds = new List<Guid>();
			var startDate = DateTime.MaxValue;
			var endDate = DateTime.MinValue;
			foreach (var pa in personAssignments)
			{
				personIds.Add(pa.Person.Id.Value);
				startDate = new [] {pa.Period.StartDateTime, startDate}.Min();
				endDate = new [] {pa.Period.EndDateTime, endDate}.Max();
			}

			_messageSender.Send(new Message
			{
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
				StartDate = Subscription.DateToString(startDate),
				EndDate = Subscription.DateToString(endDate),
				DomainReferenceId = _scenario.Current().Id.Value.ToString(),
				DomainUpdateType = (int) DomainUpdateType.NotApplicable,
				DomainType = typeof (IAggregatedScheduleChange).Name,
				BinaryData = _serializer.SerializeObject(personIds)
			});
		}
	}
}