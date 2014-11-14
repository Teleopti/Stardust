using System;
using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class RtaForTest : Web.Areas.Rta.Rta
	{
		public RtaForTest(FakeRtaDatabase database)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(MockRepository.GenerateMock<IEventPopulatingPublisher>(), new FakeCurrentDatasource()),
				new Now(),
				new FakeConfigReader()
				)
		{
		}

		public RtaForTest(FakeRtaDatabase database, INow now)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(MockRepository.GenerateMock<IEventPopulatingPublisher>(), new FakeCurrentDatasource()),
				now,
				new FakeConfigReader())
		{
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPopulatingPublisher eventPopulatingPublisher)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(eventPopulatingPublisher, new FakeCurrentDatasource()),
				now,
				new FakeConfigReader())
		{
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender)
			: this(
				new FakeSignalRClient(),
				messageSender,
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(messageSender, new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(MockRepository.GenerateMock<IEventPopulatingPublisher>(), new FakeCurrentDatasource()),
				now,
				new FakeConfigReader())
		{
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPopulatingPublisher eventPopulatingPublisher, ICurrentDataSource dataSource)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				new RtaEventPublisher(eventPopulatingPublisher, dataSource),
				now,
				new FakeConfigReader())
		{
		}

		private readonly IAdherenceAggregator _adherenceAggregator;
		private FakeRtaDatabase _database;

		public RtaForTest(
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

		public static Web.Areas.Rta.Rta Make()
		{
			return new RtaForTest(new FakeRtaDatabase());
		}

		public static Web.Areas.Rta.Rta MakeBasedOnState(ExternalUserStateForTest state)
		{
			return new RtaForTest(new FakeRtaDatabase().WithDataFromState(state).Make(), new ThisIsNow(state.Timestamp));
		}

		public static Web.Areas.Rta.Rta MakeBasedOnState(ExternalUserStateForTest state, INow now)
		{
			return new RtaForTest(new FakeRtaDatabase().WithDataFromState(state).Make(), now);
		}

		public static Web.Areas.Rta.Rta MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database)
		{
			return new RtaForTest(database, new ThisIsNow(state.Timestamp));
		}

		public static Web.Areas.Rta.Rta MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database, IEventPopulatingPublisher eventPopulatingPublisher)
		{
			return new RtaForTest(database, new ThisIsNow(state.Timestamp), eventPopulatingPublisher);
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