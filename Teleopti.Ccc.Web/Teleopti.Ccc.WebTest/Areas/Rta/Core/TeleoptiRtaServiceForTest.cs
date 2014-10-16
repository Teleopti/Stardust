using System;
using MbCache.Core;
using Rhino.Mocks;
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