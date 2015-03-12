using System;
using System.Collections.Generic;
using Autofac;
using MbCache.Core;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class RtaForTest : IRta
	{
		public RtaForTest(FakeRtaDatabase database)
		{
			buildByIoC(null, database, null, null, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now)
		{
			buildByIoC(null, database, null, now, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher)
		{
			buildByIoC(null, database, eventPublisher, now, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender)
		{
			buildByIoC(messageSender, database, null, now, null);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IEventPublisher eventPublisher, ICurrentDataSource dataSource)
		{
			buildByIoC(null, database, eventPublisher, now, dataSource);
		}

		public RtaForTest(FakeRtaDatabase database, INow now, IMessageSender messageSender, IEventPublisher eventPublisher)
		{
			buildByIoC(messageSender, database, eventPublisher, now, null);
		}

		private IRta _rta;

		private void buildByIoC(IMessageSender messageSender, FakeRtaDatabase database, IEventPublisher eventPublisher, INow now, ICurrentDataSource dataSource)
		{
			var builder = new ContainerBuilder();
			var iocConfiguration = new IocConfiguration(new IocArgs(), new FalseToggleManager());
			builder.RegisterModule(new CommonModule(iocConfiguration));
			builder.RegisterInstance(messageSender ?? MockRepository.GenerateMock<IMessageSender>());
			builder.RegisterInstance(dataSource ?? new FakeCurrentDatasource());
			builder.RegisterInstance(eventPublisher ?? new FakeEventPublisher());
			builder.RegisterInstance(now ?? new Now());
			builder.RegisterInstance(database)
				.As<IDatabaseReader>()
				.As<IDatabaseWriter>()
				.As<IPersonOrganizationReader>()
				;
			builder.RegisterInstance(database.AgentStateReadModelReader).As<IAgentStateReadModelReader>();
			builder.RegisterInstance(database.RtaStateGroupRepository).As<IRtaStateGroupRepository>();
			builder.RegisterInstance(database.StateGroupActivityAlarmRepository).As<IStateGroupActivityAlarmRepository>();
			builder.RegisterInstance(new FakeMbCacheFactory()).As<IMbCacheFactory>();
			builder.RegisterInstance(new FakeAllBusinessUnitsUnitOfWorkAspect()).As<IAllBusinessUnitsUnitOfWorkAspect>();
			var container = builder.Build();

			_rta = container.Resolve<IRta>();
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

		public void Initialize()
		{
			_rta.Initialize();
		}

	}

	public static class RtaExtensions
	{
		public static void CheckForActivityChange(this IRta rta, Guid personId, Guid businessUnitId)
		{
			rta.CheckForActivityChange(new CheckForActivityChangeInputModel
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId
			});
		}
	}
}