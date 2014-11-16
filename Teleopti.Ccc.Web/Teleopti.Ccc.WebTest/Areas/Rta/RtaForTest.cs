using System;
using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using IMessageSender = Teleopti.Interfaces.MessageBroker.Client.IMessageSender;

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
				rtaEventPublisher(),
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
				rtaEventPublisher(),
				now,
				new FakeConfigReader())
		{
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				rtaEventPublisher(eventPublisher),
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
				rtaEventPublisher(),
				now,
				new FakeConfigReader())
		{
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher, ICurrentDataSource dataSource)
			: this(
				new FakeSignalRClient(),
				MockRepository.GenerateMock<IMessageSender>(),
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				new AdherenceAggregator(MockRepository.GenerateMock<IMessageSender>(), new OrganizationForPerson(new PersonOrganizationProvider(database))),
				rtaEventPublisher(eventPublisher, dataSource),
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

		private static IRtaEventPublisher rtaEventPublisher()
		{
			return rtaEventPublisher(new FakeEventPublisher());
		}

		private static IRtaEventPublisher rtaEventPublisher(IEventPublisher eventPublisher)
		{
			return rtaEventPublisher(eventPublisher, new FakeCurrentDatasource());
		}

		private static IRtaEventPublisher rtaEventPublisher(IEventPublisher eventPublisher, ICurrentDataSource dataSource)
		{
			var populatingEventPublisher = new EventPopulatingPublisher(eventPublisher, new EventContextPopulator(null, dataSource, null));
			return new RtaEventPublisher(
				new ShiftEventPublisher(populatingEventPublisher),
				new AdherenceEventPublisher(populatingEventPublisher)
				);
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

		public static Web.Areas.Rta.Rta MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database, IEventPublisher eventPublisher)
		{
			return new RtaForTest(database, new ThisIsNow(state.Timestamp), eventPublisher);
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