using System.Collections.Generic;
using System.Linq;
using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class TeleoptiRtaServiceForTest : TeleoptiRtaService
	{

		public TeleoptiRtaServiceForTest(FakeRtaDatabase database)
			: this(
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
			: this(
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
			: this(
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

		public TeleoptiRtaServiceForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				makeActualAgentStateHasBeenSent(database, MockRepository.GenerateMock<IEventPublisher>(), messageSender),
				now,
				new FakeConfigReader())
		{
		}

		private IMessageSender _messageSender;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private FakeRtaDatabase _database;

		public TeleoptiRtaServiceForTest(
			ISignalRClient signalRClient,
			IMessageSender messageSender,
			IDatabaseReader databaseReader,
			IDatabaseWriter databaseWriter,
			IMbCacheFactory cacheFactory,
			IEnumerable<IActualAgentStateHasBeenSent> actualAgentStateHasBeenSent,
			INow now,
			IConfigReader configReader)
			: base(
				signalRClient,
				messageSender,
				databaseReader,
				databaseWriter,
				cacheFactory,
				actualAgentStateHasBeenSent,
				now,
				configReader
				)
		{
			_messageSender = messageSender;
			_adherenceAggregator = actualAgentStateHasBeenSent.OfType<IAdherenceAggregator>().Single();
			_database = databaseReader as FakeRtaDatabase;
		}

		public static TeleoptiRtaService Make()
		{
			return new TeleoptiRtaServiceForTest(new FakeRtaDatabase());
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state)
		{
			return new TeleoptiRtaServiceForTest(new FakeRtaDatabase().WithDataFromState(state).Done(), new ThisIsNow(state.Timestamp));
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state, INow now)
		{
			return new TeleoptiRtaServiceForTest(new FakeRtaDatabase().WithDataFromState(state).Done(), now);
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

		public void Initialize()
		{
			new AdherenceAggregatorInitializor(
				_adherenceAggregator,
				_database
				).Initialize();
		}
	}
}