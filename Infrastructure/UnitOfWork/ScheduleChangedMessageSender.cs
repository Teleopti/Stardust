using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	[RemoveMeWithToggle(Toggles.MessageBroker_ScheduleChangedMessagePackaging_79140)]
	public class ScheduleChangedMessageSender : ITransactionHook
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

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
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

			var scenarios = persistableScheduleDatas.GroupBy(s => s.Scenario);
			var startDateTime = DateTime.MaxValue;
			var endDateTime = DateTime.MinValue;
			var personIds = new HashSet<Guid>();
			foreach (var scenario in scenarios)
			{
				if (scenario.Key == null) continue;
				foreach (var person in scenario.GroupBy(s => s.Person))
				{
					if (!person.Any()) continue;

					startDateTime = new[] { person.Min(s => s.Period.StartDateTime), startDateTime }.Min();
					endDateTime = new[] { person.Max(s => s.Period.EndDateTime), endDateTime }.Max();
					personIds.Add(person.Key.Id.Value);
				}
				var initiatorId = Guid.Empty.ToString();
				var identifier = initiatorIdentifier();
				if (identifier != null)
					initiatorId = identifier.InitiatorId.ToString();
				var messge = new Message
				{
					ModuleId = initiatorId,
					DataSource = _dataSource.CurrentName(),
					BusinessUnitId = _businessUnit.Current() == null ? null : Subscription.IdToString(_businessUnit.Current().Id.Value),
					StartDate = Subscription.DateToString(startDateTime),
					EndDate = Subscription.DateToString(endDateTime),
					DomainReferenceId = scenario.Key.Id.Value.ToString(),
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