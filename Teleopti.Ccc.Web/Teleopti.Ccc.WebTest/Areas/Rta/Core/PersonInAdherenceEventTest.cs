using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	[TestFixture]
	public class PersonInAdherenceEventTest
	{
		[Test,Ignore]
		public void ShouldPublishPersonInAdherenceEvent() // (...WhenNoStaffingEffect)
		{
			var database = new FakeDatabaseReader();
			var personId = Guid.NewGuid();
			database.AddTestData("sourceId", "usercode", personId, Guid.NewGuid());

			var messageSender = MockRepository.GenerateMock<IMessageSender>();
			var publisher = new FakeEventsPublisher();
			var cacheFactory = MockRepository.GenerateMock<IMbCacheFactory>();
			var target = new TeleoptiRtaService(
				new RtaDataHandler(
					new FakeSignalRClient(),
					MockRepository.GenerateMock<IMessageSender>(),
					new DataSourceResolver(database),
					new PersonResolver(database),
					new ActualAgentAssembler(
						database,
						new CurrentAndNextLayerExtractor(),
						MockRepository.GenerateMock<IMbCacheFactory>(),
						new AlarmMapper(database, database, cacheFactory)
						),
					database,
					new IActualAgentStateHasBeenSent[]
					{
						new AdherenceAggregator(
							messageSender,
							new OrganizationForPerson(new PersonOrganizationProvider(database))
							),
						new AgentStateChangedCommandHandler(publisher)
					}),
				new TestableNow(),
				new ConfigReader()
				);

			//var state = new ActualAgentState
			//{
			//	PersonId = Guid.NewGuid(),
			//	StaffingEffect = 0
			//};

			//target.Invoke(state);

			target.SaveExternalUserState("!#¤atAbgT%", "usercode", "statecode", "", true, 0, DateTime.UtcNow, Guid.NewGuid().ToString(), "sourceId", DateTime.UtcNow, false);

			var @event = publisher.PublishedEvents.Single() as PersonInAdherenceEvent;
			@event.PersonId.Should().Be(personId);

		}
	}

	public class FakeDatabaseReader : IDatabaseReader, IDatabaseWriter, IPersonOrganizationReader
	{
		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>> _externalLogOns = new List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>>();

		public void AddTestData(string sourceId, string userCode, Guid personId, Guid businessUnitId)
		{
			const int datasourceId = 0;
			_datasources.Add(new KeyValuePair<string, int>(sourceId, datasourceId));
			var lookupKey = string.Format("{0}|{1}", datasourceId, userCode).ToUpper(); //putting this logic here is just WRONG
			_externalLogOns.Add(new KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>(lookupKey, new[] { new PersonWithBusinessUnit { PersonId = personId, BusinessUnitId = businessUnitId } }));

		}

		public IActualAgentState GetCurrentActualAgentState(Guid personId)
		{
			return null;
		}

		public ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> StateGroups()
		{
			return new ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>();
		}

		public ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> ActivityAlarms()
		{
			return new ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>();
		}

		public IList<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			return new List<ScheduleLayer>();
		}

		public IEnumerable<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			yield break;
		}

		public ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>> ExternalLogOns()
		{
			return new ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>>(_externalLogOns);
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return new ConcurrentDictionary<string, int>(_datasources);
		}

		public RtaStateGroupLight AddAndGetNewRtaState(string stateCode, Guid platformTypeId, Guid businessUnit)
		{
			return null;
		}

		public void AddOrUpdate(IActualAgentState actualAgentState)
		{
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			yield break;
		}
	}

	public class FakeSignalRClient : ISignalRClient
	{

		public void Configure(string url)
		{
			Url = url;
		}

		public string Url { get; private set; }

		public bool IsAlive { get { return true; } }

		public void StartBrokerService(bool useLongPolling = false)
		{
		}

		public void Call(string methodName, params object[] args)
		{
		}

		public void RegisterCallbacks(Action<Notification> onNotification, Action afterConnectionCreated)
		{
		}

		public void Dispose()
		{
		}
	}

	public class FakeEventsPublisher : IEventPublisher
	{
		public IList<IEvent> PublishedEvents = new List<IEvent>();

		public void Publish(IEvent @event)
		{
			PublishedEvents.Add(@event);
		}
	}

}