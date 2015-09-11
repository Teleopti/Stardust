using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class AllBusinessUnitsUnitOfWorkTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheServiceImpl>();
		}

		public TheServiceImpl TheService;
		public IRtaStateGroupRepository RepositoryNotValidatingUserLogon;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ISiteRepository SiteRepository;
		public IPrincipalAndStateContext Context;
		public IDataSourceScope DataSource;
		public IDataSourceForTenant DataSourceForTenant;

		public class TheServiceImpl
		{
			private readonly ICurrentUnitOfWork _uow;
			private readonly IDataSourceScope _dataSource;
			private readonly IDataSourceForTenant _dataSourceForTenant;

			public TheServiceImpl(
				ICurrentUnitOfWork uow,
				IDataSourceScope dataSource,
				IDataSourceForTenant dataSourceForTenant)
			{
				_uow = uow;
				_dataSource = dataSource;
				_dataSourceForTenant = dataSourceForTenant;
			}

			public virtual void Does(Action<IUnitOfWork> action)
			{
				using (_dataSource.OnThisThreadUse("App"))
					DoesInner(action);
			}

			[UnitOfWork]
			protected virtual void DoesInner(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}

			public virtual void DoesOnAllBusinessUnits(Action<IUnitOfWork> action)
			{
				using (_dataSource.OnThisThreadUse("App"))
					DoesOnAllBusinessUnitsWithoutDatasource(action);
			}

			[AllBusinessUnitsUnitOfWork]
			public virtual void DoesOnAllBusinessUnitsWithoutDatasource(Action<IUnitOfWork> action)
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
			using (DataSource.OnThisThreadUse("App"))
			{
				TheService.DoesOnAllBusinessUnitsWithoutDatasource(uow => { });

				// existing beahvior is the HibernateException, although something else would be better
				//UnitOfWork.Current().Should().Be.Null();
				Assert.Throws<HibernateException>(() => RepositoryNotValidatingUserLogon.LoadAll());
			}

		}
		
		[Test]
		public void ShouldNotPersistOnException()
		{
			IList<IRtaState> stateCollection = null;
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