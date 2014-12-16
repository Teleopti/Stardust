using System;
using System.Collections.Generic;
using Autofac;
using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using IMessageSender = Teleopti.Interfaces.MessageBroker.Client.IMessageSender;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class RtaForTest : IRta
	{
		public RtaForTest(FakeRtaDatabase database)
		{
			buildLikeAnIoC(null, database, null, null, null, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now)
		{
			buildLikeAnIoC(null, database, null, now, null, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher)
		{
			buildLikeAnIoC(null, database, eventPublisher, now, null, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender)
		{
			buildLikeAnIoC(messageSender, database, null, now, null, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher, ICurrentDataSource dataSource)
		{
			buildLikeAnIoC(null, database, eventPublisher, now, dataSource, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender, IEventPublisher eventPublisher)
		{
			buildLikeAnIoC(messageSender, database, eventPublisher, now, null, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender, IToggleManager toggles)
		{
			buildLikeAnIoC(messageSender, database, null, now, null, toggles);
		}

		private IRta _rta;
		private IAdherenceAggregatorInitializor _initializor;

		private void buildLikeAnIoC(IMessageSender messageSender, FakeRtaDatabase database, IEventPublisher eventPublisher, INow now, ICurrentDataSource dataSource, IToggleManager toggles)
		{
			toggles = toggles ?? new FakeToggleManager();
			var builder = new ContainerBuilder();
			var iocConfiguration = new IocConfiguration(new IocArgs(), toggles);
			builder.RegisterModule(new CommonModule(iocConfiguration));
			builder.RegisterModule(new RtaModule(iocConfiguration));
			builder.RegisterInstance(messageSender ?? MockRepository.GenerateMock<IMessageSender>());
			builder.RegisterInstance(dataSource ?? new FakeCurrentDatasource());
			builder.RegisterInstance(eventPublisher ?? new FakeEventPublisher());
			builder.RegisterInstance(now ?? new Now());
			builder.RegisterInstance(database)
				.As<IDatabaseReader>()
				.As<IDatabaseWriter>()
				.As<IPersonOrganizationReader>()
				.As<IReadActualAgentStates>()
				;
			builder.RegisterInstance(new FakeMbCacheFactory()).As<IMbCacheFactory>();
			var container = builder.Build();

			_rta = container.Resolve<IRta>();
			_initializor = container.Resolve<IAdherenceAggregatorInitializor>();
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
			_initializor.Initialize();
		}

	}
}