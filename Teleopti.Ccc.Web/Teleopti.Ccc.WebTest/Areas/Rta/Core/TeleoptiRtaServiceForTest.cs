using System;
using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	public class TeleoptiRtaServiceForTest : TeleoptiRtaService
	{
		public TeleoptiRtaServiceForTest(ExternalUserStateForTest state, FakeRtaDatabase database)
			: base(MakeRtaDataHandler(database), new ThisIsNow(state.Timestamp), new FakeConfigReader())
		{
		}
		public TeleoptiRtaServiceForTest(ExternalUserStateForTest state, FakeRtaDatabase database, IEventPublisher eventPublisher)
			: base(MakeRtaDataHandler(database, eventPublisher), new ThisIsNow(state.Timestamp), new FakeConfigReader())
		{
		}

		public TeleoptiRtaServiceForTest()
			: this(new ExternalUserStateForTest())
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
			return MakeRtaDataHandler(database, new FakeEventsPublisher());
		}

		private static IRtaDataHandler MakeRtaDataHandler(FakeRtaDatabase database, IEventPublisher eventPublisher)
		{
			var cacheFactory = MockRepository.GenerateMock<IMbCacheFactory>();
			var messageSender = MockRepository.GenerateMock<IMessageSender>();
			return new RtaDataHandler(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				new PersonResolver(database),
				new ActualAgentAssembler(
					database,
					new CurrentAndNextLayerExtractor(),
					MockRepository.GenerateMock<IMbCacheFactory>(),
					new AlarmMapper(database, database, cacheFactory)
					),
				database,
				database,
				new IActualAgentStateHasBeenSent[]
				{
					new AdherenceAggregator(
						messageSender,
						new OrganizationForPerson(new PersonOrganizationProvider(database))
						),
					new AgentStateChangedCommandHandler(eventPublisher)
				});
		}

	}
}