using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ServiceModel;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server
{
	[TestFixture]
	public class TeleoptiRtaServiceTest
	{
		[Test]
		public void ShouldProcessValidState()
		{
			var state = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state);

			var result = state.SaveExternalUserStateTo(target);

			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldThrowIfAuthenticationKeyIsIncorrect()
		{
			var state = new ExternalUserStateForTest { AuthenticationKey = "something" };
			var target = new TeleoptiRtaServiceForTest(state);

			state.AuthenticationKey += " else";

			Assert.Throws(typeof(FaultException), () => state.SaveExternalUserStateTo(target));
		}

		[Test]
		public void ShouldNotProcessStatesThatsToOld()
		{
			var christmas = new DateTime(2001, 12, 24, 15, 0, 0);
			var now = MockRepository.GenerateStub<INow>();
			now.Expect(n => n.UtcDateTime()).Return(christmas);
			var state = new ExternalUserStateForTest() { Timestamp = christmas.AddHours(1) };
			var target = new TeleoptiRtaServiceForTest(state, now);

			var result = state.SaveExternalUserStateTo(target);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotProcessStatesWithTimeStampNewerThanNow()
		{
			var christmas = new DateTime(2001, 12, 24, 15, 0, 0);
			var now = MockRepository.GenerateStub<INow>();
			now.Expect(n => n.UtcDateTime()).Return(christmas);
			var state = new ExternalUserStateForTest { Timestamp = christmas.Subtract(TimeSpan.FromHours(1)) };
			var target = new TeleoptiRtaServiceForTest(state, now);

			var result = state.SaveExternalUserStateTo(target);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotProcessIfNoSourceId()
		{
			var state = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state);
			state.SourceId = string.Empty;

			var result = state.SaveExternalUserStateTo(target);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldPersistActualAgentState()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			var target = new TeleoptiRtaServiceForTest(database, state);

			state.SaveExternalUserStateTo(target);

			database.PersistedActualAgentState.StateCode.Should().Be(state.StateCode);
		}

		[Test]
		public void ShouldNotProcessStateIfNoPlatformId()
		{
			var state = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state);
			state.PlatformTypeId = string.Empty;

			var result = state.SaveExternalUserStateTo(target);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldCutStateCodeIfToLong()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			var target = new TeleoptiRtaServiceForTest(database, state);

			state.StateCode = "a really really really really looooooooong statecode that should be trimmed somehow for whatever reason";
			state.SaveExternalUserStateTo(target);

			database.PersistedActualAgentState.StateCode.Should().Be(state.StateCode.Substring(0, 25));
		}

		[Test]
		public void ShouldPersistStateCodeToLoggedOutIfNotLoggedIn()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			var target = new TeleoptiRtaServiceForTest(database, state);

			state.IsLoggedOn = false;
			state.SaveExternalUserStateTo(target);

			database.PersistedActualAgentState.StateCode.Should().Be(TeleoptiRtaService.LogOutStateCode);
		}

		[Test]
		public void ShouldProcessExternalUserStatesInBatch()
		{
			var state1 = new ExternalUserStateForTest();
			var state2 = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state1);

			var result = target.SaveBatchExternalUserState(state1.AuthenticationKey, state1.PlatformTypeId, state1.SourceId, new[] { state1, state2 });

			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldThrowIfTooManyExternalUserStatesInBatch()
		{
			const int tooManyStates = 200;
			var externalStates = new Collection<ExternalUserState>();
			for (var i = 0; i < tooManyStates; i++)
				externalStates.Add(new ExternalUserStateForTest());
			var state = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state);

			Assert.Throws(typeof(FaultException), () => target.SaveBatchExternalUserState(state.AuthenticationKey, state.PlatformTypeId, state.SourceId, externalStates));
		}

		[Test]
		public void ShouldPersistActualAgentStateWhenScheduleIsUpdated()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, personId, businessUnitId);
			var target = new TeleoptiRtaServiceForTest(database, state);

			target.GetUpdatedScheduleChange(personId, businessUnitId, state.Timestamp);

			database.PersistedActualAgentState.PersonId.Should().Be(personId);
		}

		private class ExternalUserStateForTest : ExternalUserState
		{
			public string AuthenticationKey = TeleoptiRtaService.DefaultAuthenticationKey;
			public string PlatformTypeId = Guid.Empty.ToString();
			public string SourceId = "sourceId";

			public ExternalUserStateForTest()
			{
				UserCode = "8808";
				StateCode = "AUX2";
				IsLoggedOn = true;
			}

			public int SaveExternalUserStateTo(TeleoptiRtaService service)
			{
				return service.SaveExternalUserState(AuthenticationKey, PlatformTypeId, SourceId, this);
			}

		}

		private class TeleoptiRtaServiceForTest : TeleoptiRtaService
		{
			public TeleoptiRtaServiceForTest(FakeRtaDatabase database, ExternalUserStateForTest state)
				: base(MakeRtaDataHandler(database), new ThisIsNow(state.Timestamp), new FakeConfigReader())
			{
			}

			public TeleoptiRtaServiceForTest(ExternalUserStateForTest state)
				: base(MakeRtaDataHandlerForState(state), new ThisIsNow(state.Timestamp), new FakeConfigReader())
			{
			}

			public TeleoptiRtaServiceForTest(ExternalUserStateForTest state, INow now)
				: base(MakeRtaDataHandlerForState(state), now, new FakeConfigReader())
			{
			}

			private static IRtaDataHandler MakeRtaDataHandlerForState(ExternalUserStateForTest state)
			{
				var database = new FakeRtaDatabase();
				database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
				return MakeRtaDataHandler(database);
			}

			private static IRtaDataHandler MakeRtaDataHandler(FakeRtaDatabase database)
			{
				var cacheFactory = MockRepository.GenerateMock<IMbCacheFactory>();
				var messageSender = MockRepository.GenerateMock<IMessageSender>();
				var publisher = new FakeEventsPublisher();
				return new RtaDataHandler(
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
					});
			}

		}
	}

	public class FakeConfigReader : IConfigReader
	{
		public FakeConfigReader()
		{
			AppSettings = new NameValueCollection();
		}

		public NameValueCollection AppSettings { get; set; }
	}
}