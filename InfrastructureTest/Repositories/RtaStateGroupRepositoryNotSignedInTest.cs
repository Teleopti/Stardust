using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class RtaStateGroupRepositoryNotSignedInTest
	{
		public IRtaStateGroupRepository StateGroupRepository;
		public IPrincipalAndStateContext Context;
		public TheServiceImpl TheService;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<TheServiceImpl>().AsSelf().SingleInstance().ApplyAspects();
		}

		public class TheServiceImpl
		{
			private readonly ICurrentUnitOfWork _uow;

			public TheServiceImpl(ICurrentUnitOfWork uow)
			{
				_uow = uow;
			}

			[UnitOfWork]
			public virtual void DoesWhileLoggedIn(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}

			[AllBusinessUnitsUnitOfWork]
			public virtual void DoesWhileNotLoggedIn(Action<IUnitOfWork> action)
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