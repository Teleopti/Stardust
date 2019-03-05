using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Configuration;


namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class AllBusinessUnitsUnitOfWorkTest : IExtendSystem
	{		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TheServiceImpl>();
		}

		public TheServiceImpl TheService;
		public IRtaStateGroupRepository RepositoryNotValidatingUserLogon;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ISiteRepository SiteRepository;
		public IPrincipalAndStateContext Context;
		public IDataSourceScope DataSource;
		public IDataSourceForTenant DataSourceForTenant;
		public ICurrentUnitOfWork CurrentUnitOfWork;

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

			public virtual void Does(Action<IUnitOfWork> action)
			{
				using (_dataSource.OnThisThreadUse(SetupFixtureForAssembly.DataSource.DataSourceName))
					DoesInner(action);
			}

			[UnitOfWork]
			protected virtual void DoesInner(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}

			public virtual void DoesOnAllBusinessUnits(Action<IUnitOfWork> action)
			{
				using (_dataSource.OnThisThreadUse(SetupFixtureForAssembly.DataSource.DataSourceName))
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
			using (DataSource.OnThisThreadUse(SetupFixtureForAssembly.DataSource.DataSourceName))
			{
				TheService.DoesOnAllBusinessUnitsWithoutDatasource(uow => { });

				CurrentUnitOfWork.Current().Should().Be.Null();
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

					RepositoryNotValidatingUserLogon.LoadAll().Single().AddState("phone", "phone");
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