using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ScheduleChangedMessageSender : IPersistCallback
	{
		private readonly IMessageSender _messageSender;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly IJsonSerializer _serializer;
		private static readonly Type[] includedTypes = new[] { typeof(IPersonAbsence), typeof(IPersonAssignment) };
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public ScheduleChangedMessageSender(
			IMessageSender messageSender,
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

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			send(_initiatorIdentifier.Current, modifiedRoots);
		}

		public void Send(IInitiatorIdentifier initiatorIdentifier, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			send(() => initiatorIdentifier, modifiedRoots);
		}

		private void send(Func<IInitiatorIdentifier> initiatorIdentifier, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = extractScheduleChangesOnly(modifiedRoots);
			var persistableScheduleDatas = scheduleData as IPersistableScheduleData[] ?? scheduleData.ToArray();
			if (!persistableScheduleDatas.Any()) return;

			var people = persistableScheduleDatas.Select(s => s.Person).Distinct().ToArray();
			var scenarios = persistableScheduleDatas.Select(s => s.Scenario).Distinct();
			var startDateTime = DateTime.MaxValue;
			var endDateTime = DateTime.MinValue;
			var personIds = new HashSet<Guid>();
			foreach (var scenario in scenarios)
			{
				if (scenario == null) continue;
				foreach (var person in people)
				{
					var matchedItems =
						persistableScheduleDatas.Where(s =>  s.Person.Equals(person) && s.Scenario != null && s.Scenario.Equals(scenario))
							.ToArray();
					if (!matchedItems.Any()) continue;

					startDateTime = new[] { matchedItems.Min(s => s.Period.StartDateTime), startDateTime }.Min();
					endDateTime = new[] { matchedItems.Max(s => s.Period.EndDateTime), endDateTime }.Max();
					personIds.Add(person.Id.Value);
				}
				var initiatorId = Guid.Empty.ToString();
				var identifier = initiatorIdentifier();
				if (identifier != null)
					initiatorId = identifier.InitiatorId.ToString();
				var messge = new Message
				{
					ModuleId = initiatorId,
					DataSource = _dataSource.CurrentName(),
					BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
					StartDate = Subscription.DateToString(startDateTime),
					EndDate = Subscription.DateToString(endDateTime),
					DomainReferenceId = scenario.Id.Value.ToString(),
					DomainUpdateType = (int)DomainUpdateType.NotApplicable,
					DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
					DomainType = typeof(IScheduleChangedMessage).Name,
					BinaryData = _serializer.SerializeObject(personIds)
				};

				_messageSender.Send(messge);
			}
		}

		private static IEnumerable<IPersistableScheduleData> extractScheduleChangesOnly(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = modifiedRoots.Select(r => r.Root).OfType<IPersistableScheduleData>();
			scheduleData = scheduleData.Where(s => includedTypes.Any(t => s.GetType().GetInterfaces().Contains(t)));
			return scheduleData;
		}
	}
}