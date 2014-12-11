using System;
using System.Collections.Generic;
using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using IMessageSender = Teleopti.Interfaces.MessageBroker.Client.IMessageSender;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class RtaForTest : IRta
	{
		public RtaForTest(FakeRtaDatabase database)
		{
			buildLikeAnIoC(null, database, null, null, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now)
		{
			buildLikeAnIoC(null, database, null, now, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher)
		{
			buildLikeAnIoC(null, database, eventPublisher, now, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender)
		{
			buildLikeAnIoC(messageSender, database, null, now, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher, ICurrentDataSource dataSource)
		{
			buildLikeAnIoC(null, database, eventPublisher, now, dataSource);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender, IEventPublisher eventPublisher)
		{
			buildLikeAnIoC(messageSender, database, eventPublisher, now, null);
		}

		private Web.Areas.Rta.Rta _rta;
		private AdherenceAggregator _adherenceAggregator;
		private FakeRtaDatabase _database;

		private void buildLikeAnIoC(IMessageSender messageSender, FakeRtaDatabase database, IEventPublisher eventPublisher, INow now, ICurrentDataSource dataSource)
		{
			messageSender = messageSender ?? MockRepository.GenerateMock<IMessageSender>();
			dataSource = dataSource ?? new FakeCurrentDatasource();
			eventPublisher = eventPublisher ?? new FakeEventPublisher();
			now = now ?? new Now();

			var eventPopulatingPublisher = new EventPopulatingPublisher(eventPublisher, new EventContextPopulator(null, dataSource, null));
			var adherenceEventPublisher = new AdherenceEventPublisher(eventPopulatingPublisher);

			_adherenceAggregator = new AdherenceAggregator(messageSender, new OrganizationForPerson(new PersonOrganizationProvider(database)));
			_database = database;

			_rta = new Web.Areas.Rta.Rta(
				messageSender,
				database,
				database,
				MockRepository.GenerateMock<IMbCacheFactory>(),
				_adherenceAggregator,
				new ShiftEventPublisher(eventPopulatingPublisher),
				new ActivityEventPublisher(eventPopulatingPublisher, adherenceEventPublisher),
				new StateEventPublisher(eventPopulatingPublisher, adherenceEventPublisher),
				now,
				new FakeConfigReader());
		}

		public static RtaForTest Make()
		{
			return new RtaForTest(new FakeRtaDatabase());
		}

		public static RtaForTest MakeBasedOnState(ExternalUserStateForTest state)
		{
			return new RtaForTest(new FakeRtaDatabase().WithDataFromState(state).Make(), new ThisIsNow("2014-10-20 8:00"));
		}

		public static RtaForTest MakeBasedOnState(ExternalUserStateForTest state, INow now)
		{
			return new RtaForTest(new FakeRtaDatabase().WithDataFromState(state).Make(), now);
		}

		public static RtaForTest MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database)
		{
			return new RtaForTest(database, new ThisIsNow("2014-10-20 8:00"));
		}

		//public static RtaForTest MakeBasedOnState(ExternalUserStateForTest state, FakeRtaDatabase database, IEventPublisher eventPublisher)
		//{
		//	return new RtaForTest(database, new ThisIsNow(state.Timestamp), eventPublisher);
		//}

		public int SaveState(ExternalUserStateInputModel input)
		{
			return _rta.SaveState(input);
		}

		public int SaveStateBatch(IEnumerable<ExternalUserStateInputModel> states)
		{
			return _rta.SaveStateBatch(states);
		}

		public int SaveStateSnapshot(IEnumerable<ExternalUserStateInputModel> states)
		{
			return _rta.SaveStateSnapshot(states);
		}

		public void CheckForActivityChange(CheckForActivityChangeInputModel input)
		{
			_rta.CheckForActivityChange(input);
		}

		public void CheckForActivityChange(Guid personId, Guid businessUnitId)
		{
			_rta.CheckForActivityChange(new CheckForActivityChangeInputModel
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId
			});
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