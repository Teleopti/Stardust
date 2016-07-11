using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[PrincipalAndStateTest]
	[Toggle(Toggles.RTA_Optimize_39667)]
	public class UnrecognizedStatesTest: ISetup
	{
		public IRtaStateGroupRepository StateGroupRepository;
		public PersonCreator PersonCreator;
		public IPrincipalAndStateContext Context;
		public MutableNow Now;
		public InfrastructureTest.TheServiceImpl TheService;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public ConcurrencyRunner Runner;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<InfrastructureTest.TheServiceImpl>();
			system.AddService<PersonCreator>();
		}


		[Test]
		public void ShouldAddMissingStateCodeToDefaultStateGroup()
		{
			Now.Is("2015-05-13 08:00");
			TheService.DoesWhileLoggedIn(uow =>
			{
				var rtaStateGroup = new RtaStateGroup("Phone", true, true);
				StateGroupRepository.Add(rtaStateGroup);
				20.Times
					(i => PersonCreator.CreatePersonWithExternalLogOn(Now, i.ToString()));
				uow.PersistAll();
			});
			Context.Logout();
			var states = Enumerable.Range(0, 20).Select(i => new ExternalUserStateInputModel
			{
				AuthenticationKey = "!#¤atAbgT%",
				PlatformTypeId = Guid.Empty.ToString(),
				StateCode = "InCall",
				StateDescription = "InCall",
				UserCode = i.ToString(),
				SourceId = "-1",
				IsLoggedOn = true
			});

			Target.SaveStateBatch(states);

			TheService.DoesWhileNotLoggedIn(uow =>
				StateGroupRepository.LoadAllCompleteGraph().Single().StateCollection.Should().Have.Count.EqualTo(1)
				);
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