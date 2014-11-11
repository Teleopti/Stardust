using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
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
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(MockRepository.GenerateMock<IEventPublisher>(), new FakeCurrentDatasource()),
				new Now(),
				new FakeConfigReader()
				)
		{
		}

		public TeleoptiRtaServiceForTest(FakeRtaDatabase database, INow now)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(MockRepository.GenerateMock<IEventPublisher>(), new FakeCurrentDatasource()),
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
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(eventPublisher, new FakeCurrentDatasource()),
				now,
				new FakeConfigReader())
		{
		}

		public TeleoptiRtaServiceForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender)
			: this(
				new FakeSignalRClient(),
				messageSender,
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(messageSender, new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(MockRepository.GenerateMock<IEventPublisher>(), new FakeCurrentDatasource()),
				now,
				new FakeConfigReader())
		{
		}

		public TeleoptiRtaServiceForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher, ICurrentDataSource dataSource)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(eventPublisher, dataSource),
				now,
				new FakeConfigReader())
		{
		}

		private readonly IAdherenceAggregator _adherenceAggregator;
		private FakeRtaDatabase _database;

		public TeleoptiRtaServiceForTest(
			ISignalRClient signalRClient,
			IMessageSender messageSender,
			IDatabaseReader databaseReader,
			IDatabaseWriter databaseWriter,
			IMbCacheFactory cacheFactory,
			IAdherenceAggregator adherenceAggregator,
			IRtaEventPublisher eventPublisher,
			INow now,
			IConfigReader configReader)
			: base(
				signalRClient,
				messageSender,
				databaseReader,
				databaseWriter,
				cacheFactory,
				adherenceAggregator,
				eventPublisher,
				now,
				configReader
				)
		{
			_adherenceAggregator = adherenceAggregator;
			_database = databaseReader as FakeRtaDatabase;
		}

		public static TeleoptiRtaService Make()
		{
			return new TeleoptiRtaServiceForTest(new FakeRtaDatabase());
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state)
		{
			return new TeleoptiRtaServiceForTest(new FakeRtaDatabase().WithDataFromState(state).Make(), new ThisIsNow(state.Timestamp));
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state, INow now)
		{
			return new TeleoptiRtaServiceForTest(new FakeRtaDatabase().WithDataFromState(state).Make(), now);
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database)
		{
			return new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp));
		}

		public static TeleoptiRtaService MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database, IEventPublisher eventPublisher)
		{
			return new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), eventPublisher);
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