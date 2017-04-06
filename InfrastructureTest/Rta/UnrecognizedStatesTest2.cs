using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class UnrecognizedStatesTest2 : ISetup
	{
		public IRtaStateGroupRepository StateGroupRepository;
		public IPrincipalAndStateContext Context;
		public MutableNow Now;
		public TheServiceImpl TheService;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public Database Database;
		public AnalyticsDatabase Analytics;
		public ConfigurableSyncEventPublisher Publisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
			system.AddService<TheServiceImpl>();
		}
		
		[Test]
		public void ShouldNotAddDuplicatesInBatch()
		{
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			Publisher.AddHandler(typeof(CurrentScheduleReadModelUpdater));
			Publisher.AddHandler(typeof(ExternalLogonReadModelUpdater));
			Analytics.WithDataSource(7, new BatchForTest().SourceId);
			Database
				.WithAgent("usercode1")
				.WithAgent("usercode2")
				.WithAgent("usercode3")
				.WithStateGroup("default", true)
				.WithStateCode("default")
				.WithStateGroup("logged out", false, true)
				.WithStateCode("LOGGED-OFF")
				.PublishRecurringEvents();
			Context.Logout();

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-05-18 08:00".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "phone"
					},
					new BatchStateForTest
					{
						UserCode = "usercode2",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-05-18 08:00".Utc()
			});

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-05-18 08:05".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode3",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-05-18 08:05".Utc()
			});

			TheService.DoesWhileNotLoggedIn(uow =>
			{
				var stateCodes = from g in StateGroupRepository.LoadAll()
					from s in g.StateCollection
					select s;
				stateCodes.Where(x => x.StateCode == Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
					.Should().Have.Count.EqualTo(1);
			});

		}

		public class TheServiceImpl
		{
			private readonly ICurrentUnitOfWork _uow;
			private readonly IDataSourceScope _dataSource;

			public TheServiceImpl(
				ICurrentUnitOfWork uow,
				IDataSourceScope dataSource)
			{
				_uow = uow;
				_dataSource = dataSource;
			}

			[UnitOfWork]
			public virtual void DoesWhileLoggedIn(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}

			public virtual void DoesWhileNotLoggedIn(Action<IUnitOfWork> action)
			{
				using (_dataSource.OnThisThreadUse(SetupFixtureForAssembly.DataSource.DataSourceName))
					DoesWhileNotLoggedInInner(action);
			}

			[AllBusinessUnitsUnitOfWork]
			protected virtual void DoesWhileNotLoggedInInner(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}
		}
		
	}
}