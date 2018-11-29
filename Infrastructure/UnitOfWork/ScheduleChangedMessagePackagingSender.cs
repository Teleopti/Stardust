using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ScheduleChangedMessagePackagingSender : ITransactionHook
	{
		private readonly IMessageSender _messageSender;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly IJsonSerializer _serializer;
		private static readonly Type[] includedTypes = new[] {typeof(IPersonAbsence), typeof(IPersonAssignment)};
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;
		private readonly ITime _time;
		private readonly IConfigReader _config;
		private IDisposable _timer;
		private readonly object _timerLock = new object();
		private readonly TimeSpan _sendMessageInterval;
		private readonly ConcurrentDictionary<packageKey, package> _packagedMessages = new ConcurrentDictionary<packageKey, package>();

		public ScheduleChangedMessagePackagingSender(
			IMessageSender messageSender,
			ICurrentDataSource dataSource,
			ICurrentBusinessUnit businessUnit,
			IJsonSerializer serializer,
			ICurrentInitiatorIdentifier initiatorIdentifier,
			ITime time,
			IConfigReader config)
		{
			_messageSender = messageSender;
			_dataSource = dataSource;
			_businessUnit = businessUnit;
			_serializer = serializer;
			_initiatorIdentifier = initiatorIdentifier;
			_time = time;
			_config = config;
			_sendMessageInterval = TimeSpan.FromMilliseconds(config.ReadValue("ScheduleChangedMessagePackagingSendIntervalMilliseconds", 1000));
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
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

					startDateTime = new[] {person.Min(s => s.Period.StartDateTime), startDateTime}.Min();
					endDateTime = new[] {person.Max(s => s.Period.EndDateTime), endDateTime}.Max();
					personIds.Add(person.Key.Id.Value);
				}

				var initiatorId = Guid.Empty.ToString();
				var identifier = _initiatorIdentifier.Current();
				if (identifier != null)
					initiatorId = identifier.InitiatorId.ToString();

				_packagedMessages.AddOrUpdate(new packageKey
				{
					ThreadId = Thread.CurrentThread.ManagedThreadId,
					InitiatorId = initiatorId,
					DataSource = _dataSource.CurrentName(),
					BusinessUnitId = _businessUnit.Current() == null ? null : Subscription.IdToString(_businessUnit.Current().Id.Value),
					ScenarioId = scenario.Key.Id.ToString()
				}, packageKey => new package
				{
					StartDateTime = startDateTime,
					EndDateTime = endDateTime,
					PersonIds = new List<Guid>(personIds)
				}, (packageKey, package) =>
				{
					package.StartDateTime = new[] {package.StartDateTime, startDateTime}.Min();
					package.EndDateTime = new[] {package.EndDateTime, endDateTime}.Max();
					package.PersonIds.AddRange(personIds);
					return package;
				});

				ensurePackaging();
			}
		}

		private void ensurePackaging()
		{
			if (_timer == null)
				_timer = _time.StartTimerWithLock(sendMessage, _timerLock, _sendMessageInterval);
		}

		private void sendMessage()
		{
			_packagedMessages.Keys.ForEach(k =>
			{
				if (_packagedMessages.TryRemove(k, out var package))
				{
					var message = new Message
					{
						ModuleId = k.InitiatorId,
						DataSource = k.DataSource,
						BusinessUnitId = k.BusinessUnitId,
						StartDate = Subscription.DateToString(package.StartDateTime.Value),
						EndDate = Subscription.DateToString(package.EndDateTime.Value),
						DomainReferenceId = k.ScenarioId,
						DomainUpdateType = (int) DomainUpdateType.NotApplicable,
						DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
						DomainType = typeof(IScheduleChangedMessage).Name,
						BinaryData = _serializer.SerializeObject(package.PersonIds)
					};
					_messageSender.Send(message);
				}
			});
		}

		private static IEnumerable<IPersistableScheduleData> extractScheduleChangesOnly(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = modifiedRoots.Select(r => r.Root).OfType<IPersistableScheduleData>();
			scheduleData = scheduleData.Where(s => includedTypes.Any(t => s.GetType().GetInterfaces().Contains(t)));
			return scheduleData;
		}

		private class packageKey
		{
			public int ThreadId;
			public string InitiatorId;
			public string DataSource;
			public string BusinessUnitId;
			public string ScenarioId;

			protected bool Equals(packageKey other)
			{
				return ThreadId == other.ThreadId
					   && string.Equals(InitiatorId, other.InitiatorId)
					   && string.Equals(DataSource, other.DataSource)
					   && string.Equals(BusinessUnitId, other.BusinessUnitId)
					   && string.Equals(ScenarioId, other.ScenarioId);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((packageKey) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = ThreadId;
					hashCode = (hashCode * 397) ^ (InitiatorId != null ? InitiatorId.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (DataSource != null ? DataSource.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (BusinessUnitId != null ? BusinessUnitId.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (ScenarioId != null ? ScenarioId.GetHashCode() : 0);
					return hashCode;
				}
			}
		}

		private class package
		{
			public DateTime? StartDateTime;
			public DateTime? EndDateTime;
			public List<Guid> PersonIds = new List<Guid>();
		}
	}
}