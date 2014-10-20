using System;
using System.Collections.Generic;
using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	public class TeleoptiRtaServiceForTest : TeleoptiRtaService
	{
		public TeleoptiRtaServiceForTest(FakeRtaDatabase database)
			: base(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				makeActualAgentStateHasBeenSent(database, MockRepository.GenerateMock<IEventPublisher>(), MockRepository.GenerateMock<IMessageSender>()),
				new Now(),
				new FakeConfigReader())
		{
		}

		public TeleoptiRtaServiceForTest(FakeRtaDatabase database, INow now)
			: base(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				makeActualAgentStateHasBeenSent(database, MockRepository.GenerateMock<IEventPublisher>(), MockRepository.GenerateMock<IMessageSender>()),
				now,
				new FakeConfigReader())
		{
		}

		public TeleoptiRtaServiceForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher)
			: base(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				makeActualAgentStateHasBeenSent(database, eventPublisher, MockRepository.GenerateMock<IMessageSender>()),
				now,
				new FakeConfigReader())
		{
		}

		public static TeleoptiRtaService Make()
		{
			return new TeleoptiRtaServiceForTest(new FakeRtaDatabase());
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state)
		{
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			return new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp));
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state, INow now)
		{
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			return new TeleoptiRtaServiceForTest(database, now);
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database)
		{
			return new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp));
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database, IEventPublisher eventPublisher)
		{
			return new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), eventPublisher);
		}

		private static IEnumerable<IActualAgentStateHasBeenSent> makeActualAgentStateHasBeenSent(FakeRtaDatabase database, IEventPublisher eventPublisher, IMessageSender messageSender)
		{
			return new IActualAgentStateHasBeenSent[]
			{
				new AdherenceAggregator(
					messageSender,
					new OrganizationForPerson(new PersonOrganizationProvider(database))
					),
				new AgentStateChangedCommandHandler(eventPublisher)
			};
		}
	}
}