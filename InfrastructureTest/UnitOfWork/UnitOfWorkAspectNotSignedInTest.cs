using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class UnitOfWorkAspectNotSignedInTest : IRegisterInContainer
	{
		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<TheServiceImpl>().AsSelf().SingleInstance().ApplyAspects();
			builder.RegisterType<MutableFakeCurrentHttpContext>().AsSelf().As<ICurrentHttpContext>().SingleInstance();
		}

		public TheServiceImpl TheService;
		public IRtaStateGroupRepository RepositoryNotValidatingUserLogon;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ISiteRepository SiteRepository;
		public IPrincipalAndStateContext Context;

		public class TheServiceImpl
		{
			private readonly ICurrentUnitOfWork _uow;

			public TheServiceImpl(ICurrentUnitOfWork uow)
			{
				_uow = uow;
			}

			[UnitOfWork]
			public virtual void Does(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}

			[AllBusinessUnitsUnitOfWork]
			public virtual void DoesWithoutBusinessUnitFilter(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}
		}

		[Test]
		public void ShouldQueryRepositoryNotValidatingUserLogon()
		{
			IEnumerable<IRtaStateGroup> stateGroups = null;
			var stateGroup = new RtaStateGroup(" ", true, true);

			TheService.Does(uow =>
			{
				RepositoryNotValidatingUserLogon.Add(stateGroup);
			});
			Context.Logout();
			TheService.Does(uow =>
			{
				stateGroups = RepositoryNotValidatingUserLogon.LoadAll();
			});

			stateGroups.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotFilterOnBusinessUnit()
		{
			IEnumerable<IRtaStateGroup> stateGroups = null;
			var stateGroup = new RtaStateGroup(" ", true, true);

			TheService.Does(uow =>
			{
				RepositoryNotValidatingUserLogon.Add(stateGroup);
			});
			Context.Logout();
			TheService.DoesWithoutBusinessUnitFilter(uow =>
			{
				stateGroups = RepositoryNotValidatingUserLogon.LoadAll();
			});

			stateGroups.Where(s => s.Id.Equals(stateGroup.Id.Value)).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldDisposeUnitOfWorkAfterNotFilteringOnBusinessUnit()
		{
			Context.Logout();
			TheService.DoesWithoutBusinessUnitFilter(uow => { });

			// existing beahvior is the HibernateException, although something else would be better
			//UnitOfWork.Current().Should().Be.Null();
			Assert.Throws<HibernateException>(() => RepositoryNotValidatingUserLogon.LoadAll());
		}
	}

	
}