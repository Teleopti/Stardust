using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class AggregatedScheduleChangeMessageSender : IMessageSender
	{
		private readonly Interfaces.MessageBroker.Client.IMessageSender _messageSender;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly IJsonSerializer _serializer;
		private static readonly Type[] includedTypes = new[] { typeof(IPersonAbsence), typeof(IPersonAssignment) };
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public AggregatedScheduleChangeMessageSender(
			Interfaces.MessageBroker.Client.IMessageSender messageSender,
			ICurrentDataSource dataSource,
			ICurrentBusinessUnit businessUnit,
			IJsonSerializer serializer,
			ICurrentInitiatorIdentifier initiatorIdentifier)
		{
			_messageSender = messageSender;
			_dataSource = dataSource;
			_businessUnit = businessUnit;
			_serializer = serializer;
			_initiatorIdentifier = initiatorIdentifier;
		}
		
		public void Execute(IInitiatorIdentifier initiatorIdentifier, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = extractScheduleChangesOnly(modifiedRoots);
			if (!scheduleData.Any()) return;

			var people = scheduleData.Select(s => s.Person).Distinct();
			var scenarios = scheduleData.Select(s => s.Scenario).Distinct();
			var startDateTime = DateTime.MaxValue;
			var endDateTime = DateTime.MinValue;
			var personIds = new HashSet<Guid>();
			foreach (var scenario in scenarios)
			{
				if (scenario == null) continue;
				foreach (var person in people)
				{
					var matchedItems =
						scheduleData.Where(s => s.Scenario != null && s.Scenario.Equals(scenario) && s.Person.Equals(person)).ToArray();
					if (!matchedItems.Any()) continue;

					startDateTime = new[] { matchedItems.Min(s => s.Period.StartDateTime), startDateTime }.Min();
					endDateTime = new[] { matchedItems.Max(s => s.Period.EndDateTime), endDateTime }.Max();
					personIds.Add(person.Id.Value);
				}
				var initiatorId = Guid.Empty.ToString();
				if (initiatorIdentifier != null)
					initiatorId = initiatorIdentifier.InitiatorId.ToString();
				var messge = new Message
				{
					ModuleId = initiatorId,
					DataSource = _dataSource.CurrentName(),
					BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
					StartDate = Subscription.DateToString(startDateTime),
					EndDate = Subscription.DateToString(endDateTime),
					DomainReferenceId = scenario.Id.Value.ToString(),
					DomainUpdateType = (int)DomainUpdateType.NotApplicable,
					DomainQualifiedType = typeof(IAggregatedScheduleChange).AssemblyQualifiedName,
					DomainType = typeof(IAggregatedScheduleChange).Name,
					BinaryData = _serializer.SerializeObject(personIds)
				};

				_messageSender.Send(messge);
			}
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			Execute(_initiatorIdentifier.Current(), modifiedRoots);
		}

		private static IEnumerable<IPersistableScheduleData> extractScheduleChangesOnly(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = modifiedRoots.Select(r => r.Root).OfType<IPersistableScheduleData>();
			scheduleData = scheduleData.Where(s => includedTypes.Any(t => s.GetType().GetInterfaces().Contains(t)));
			return scheduleData;
		}
	}
}