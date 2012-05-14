﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration.Cache
{
	[TestFixture]
	[Category("LongRunning")]
	public class PersonInApplicationRoleTest
	{
		private IDataSource dataSource;
		private IPerson person;
		private IApplicationRole applicationRole;

		[Test]
		public void ShouldCachePersonCollection()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory) dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var p = new PersonRepository(uow).Get(person.Id.Value);
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
				var p = new PersonRepository(uow).Get(person.Id.Value);
				sessionFactory.Statistics.Clear();
				LazyLoadingManager.Initialize(p.PermissionInformation.ApplicationRoleCollection);
			}
			sessionFactory.Statistics.EntityLoadCount.Should().Be.EqualTo(0);
		}

		[SetUp]
		public void Setup()
		{
			var dsFactory = new DataSourcesFactory(new EnversConfiguration(), new List<IDenormalizer>(), new DataSourceConfigurationSetter(true, false, "call"));
			dataSource = dsFactory.Create(SetupFixtureForAssembly.Sql2005conf(ConnectionStringHelper.ConnectionStringUsedInTests, null), null);
			applicationRole = new ApplicationRole { Name = "hejhej" };
			person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.Local));
			person.PermissionInformation.AddApplicationRole(applicationRole);
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				new ApplicationRoleRepository(uow).Add(applicationRole);
				new PersonRepository(uow).Add(person);
				uow.PersistAll();
			}

			//fill cache
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var p = new PersonRepository(uow).Get(person.Id.Value);
				LazyLoadingManager.Initialize(p.PermissionInformation.ApplicationRoleCollection);
			}
		}

		[TearDown]
		public void Teardown()
		{
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				new ApplicationRoleRepository(uow).Remove(applicationRole);
				new PersonRepository(uow).Remove(person);
				uow.PersistAll();
			}
		}
	}
}