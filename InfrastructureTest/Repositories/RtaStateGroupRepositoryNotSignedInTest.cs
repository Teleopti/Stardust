using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class RtaStateGroupRepositoryNotSignedInTest : ISetup
	{
		public IRtaStateGroupRepository StateGroupRepository;
		public IPrincipalAndStateContext Context;
		public TheServiceImpl TheService;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheServiceImpl>();
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
				using (_dataSource.OnThisThreadUse("App"))
					DoesWhileNotLoggedInInner(action);
			}

			[AllBusinessUnitsUnitOfWork]
			protected virtual void DoesWhileNotLoggedInInner(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}
		}

		[Test]
		public void ShouldAddUnrecognizedStateCodeToDefaultStateGroup()
		{
			TheService.DoesWhileLoggedIn(uow => 
				StateGroupRepository.Add(new RtaStateGroup(" ", true, true)));
			Context.Logout();

			TheService.DoesWhileNotLoggedIn(uow => 
				StateGroupRepository.LoadAll().Single().AddState("phone", "phone", Guid.NewGuid()));

			TheService.DoesWhileNotLoggedIn(uow => 
				StateGroupRepository.LoadAll().Single().StateCollection.Single()
					.StateCode.Should().Be.EqualTo("phone"));
		}

		[Test]
		public void ShouldAddUnregognizedStateCodeToDefaultStateGroupWhenLoggedIn()
		{
			TheService.DoesWhileLoggedIn(uow => 
				StateGroupRepository.Add(new RtaStateGroup(" ", true, true)));

			TheService.DoesWhileNotLoggedIn(uow => 
				StateGroupRepository.LoadAll().Single().AddState("phone", "phone", Guid.NewGuid()));

			TheService.DoesWhileNotLoggedIn(uow => 
				StateGroupRepository.LoadAll().Single().StateCollection.Single()
					.StateCode.Should().Be.EqualTo("phone"));
		}

	}
}