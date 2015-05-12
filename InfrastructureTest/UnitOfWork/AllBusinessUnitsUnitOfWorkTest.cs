using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Aop;
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
	public class AllBusinessUnitsUnitOfWorkTest : IRegisterInContainer
	{
		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<TheServiceImpl>().AsSelf().SingleInstance().ApplyAspects();
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
			public virtual void DoesOnAllBusinessUnits(Action<IUnitOfWork> action)
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
			TheService.DoesOnAllBusinessUnits(uow =>
			{
				stateGroups = RepositoryNotValidatingUserLogon.LoadAll();
			});

			stateGroups.Where(s => s.Id.Equals(stateGroup.Id.Value)).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldDisposeUnitOfWorkAfterNotFilteringOnBusinessUnit()
		{
			Context.Logout();
			TheService.DoesOnAllBusinessUnits(uow => { });

			// existing beahvior is the HibernateException, although something else would be better
			//UnitOfWork.Current().Should().Be.Null();
			Assert.Throws<HibernateException>(() => RepositoryNotValidatingUserLogon.LoadAll());
		}

		[Test]
		public void ShouldPersistChanges()
		{
			ReadOnlyCollection<IRtaState> stateCollection = null;
			TheService.Does(uow =>
			{
				RepositoryNotValidatingUserLogon.Add(new RtaStateGroup(" ", true, true));
			});
			
			TheService.DoesOnAllBusinessUnits(uow =>
			{
				RepositoryNotValidatingUserLogon.LoadAll().Single().AddState("phone", "phone", Guid.NewGuid());
			});

			TheService.DoesOnAllBusinessUnits(uow =>
			{
				stateCollection = RepositoryNotValidatingUserLogon.LoadAll().Single().StateCollection;
			});
			stateCollection.Select(x => x.Name).Single().Should().Be("phone");
		}

		[Test]
		public void ShouldPersistChangesWhenNotLoggedIn()
		{
			TheService.Does(uow =>
			{
				RepositoryNotValidatingUserLogon.Add(new RtaStateGroup(" ", true, true));
			});
			Context.Logout();

			TheService.DoesOnAllBusinessUnits(uow =>
			{
				RepositoryNotValidatingUserLogon.LoadAll().Single().AddState("phone", "phone", Guid.NewGuid());
			});

			TheService.DoesOnAllBusinessUnits(uow =>
			{
				RepositoryNotValidatingUserLogon.LoadAll().Should().Have.Count.EqualTo(1);
			});
		}

		[Test]
		public void ShouldNotPersistOnException()
		{
			ReadOnlyCollection<IRtaState> stateCollection = null;
			TheService.Does(uow =>
			{
				RepositoryNotValidatingUserLogon.Add(new RtaStateGroup(" ", true, true));
			});
			try
			{
				TheService.DoesOnAllBusinessUnits(uow =>
				{

					RepositoryNotValidatingUserLogon.LoadAll().Single().AddState("phone", "phone", Guid.NewGuid());
					throw new Exception("derp!");
				});
			}
			catch { }

			TheService.DoesOnAllBusinessUnits(uow =>
			{
				stateCollection = RepositoryNotValidatingUserLogon.LoadAll().Single().StateCollection;
			});
			stateCollection.Should().Be.Empty();
		}
	}

	
}