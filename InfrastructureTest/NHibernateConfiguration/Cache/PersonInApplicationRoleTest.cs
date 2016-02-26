using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration.Cache
{
	[TestFixture]
	[Category("LongRunning")]
	public class PersonInApplicationRoleTest : DatabaseTestWithoutTransaction
	{
		private IDataSource dataSource;
		private IPerson person;
		private IApplicationRole applicationRole;

		[Test]
		public void ShouldCachePersonApplicationRoleCollection()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory) dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var p = new PersonRepository(new ThisUnitOfWork(uow)).Get(person.Id.Value);
				sessionFactory.Statistics.Clear();
				LazyLoadingManager.Initialize(p.PermissionInformation.ApplicationRoleCollection);
			}
			sessionFactory.Statistics.CollectionLoadCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldCacheApplicationRole()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory)dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var p = new PersonRepository(new ThisUnitOfWork(uow)).Get(person.Id.Value);
				sessionFactory.Statistics.Clear();
				LazyLoadingManager.Initialize(p.PermissionInformation.ApplicationRoleCollection);
			}
			sessionFactory.Statistics.CollectionLoadCount.Should().Be.EqualTo(0);
		}

		[SetUp]
		public void Setup1()
		{
			var dsFactory = new DataSourcesFactory(new EnversConfiguration(), new NoPersistCallbacks(), DataSourceConfigurationSetter.ForTestWithCache(), new CurrentHttpContext(), null);
			dataSource = dsFactory.Create(SetupFixtureForAssembly.Sql2005conf(InfraTestConfigReader.ConnectionString, null), null);
			applicationRole = new ApplicationRole { Name = "hejhej" };
			person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			person.PermissionInformation.AddApplicationRole(applicationRole);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				new ApplicationRoleRepository(uow).Add(applicationRole);
				new PersonRepository(new ThisUnitOfWork(uow)).Add(person);
				uow.PersistAll();
			}

			//fill cache
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var p = new PersonRepository(new ThisUnitOfWork(uow)).Get(person.Id.Value);
				LazyLoadingManager.Initialize(p.PermissionInformation.ApplicationRoleCollection);
			}
		}

		[TearDown]
		public void Teardown()
		{
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				new ApplicationRoleRepository(uow).Remove(applicationRole);
				new PersonRepository(new ThisUnitOfWork(uow)).Remove(person);
				uow.PersistAll();
			}
		}
	}
}