﻿using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration.Cache
{
	[TestFixture]
	[Category("LongRunning")]
	public class AvailableDataTest
	{
		private IDataSource dataSource;
		private IAvailableData availableData;
		private IBusinessUnit businessUnit;
		private ISite site;
		private ITeam team;

		[Test]
		public void AvailableDataShouldBeCached()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory)dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				sessionFactory.Statistics.Clear();
				new AvailableDataRepository(uow).Get(availableData.Id.Value);
			}
			sessionFactory.Statistics.EntityLoadCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void AvailableTeamsShouldBeCached()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory)dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var aData = new AvailableDataRepository(uow).Get(availableData.Id.Value);
				sessionFactory.Statistics.Clear();
				LazyLoadingManager.Initialize(aData.AvailableTeams);
			}
			sessionFactory.Statistics.EntityLoadCount.Should().Be.EqualTo(0);
			sessionFactory.Statistics.CollectionLoadCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void AvailableSitesShouldBeCached()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory)dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var aData = new AvailableDataRepository(uow).Get(availableData.Id.Value);
				sessionFactory.Statistics.Clear();
				LazyLoadingManager.Initialize(aData.AvailableSites);
			}
			sessionFactory.Statistics.EntityLoadCount.Should().Be.EqualTo(0);
			sessionFactory.Statistics.CollectionLoadCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void AvailableBusinessUnitShouldBeCached()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory)dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var aData = new AvailableDataRepository(uow).Get(availableData.Id.Value);
				sessionFactory.Statistics.Clear();
				LazyLoadingManager.Initialize(aData.AvailableBusinessUnits);
			}
			sessionFactory.Statistics.EntityLoadCount.Should().Be.EqualTo(0);
			sessionFactory.Statistics.CollectionLoadCount.Should().Be.EqualTo(0);
		}

		[SetUp]
		public void Setup()
		{
			var dsFactory = new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(), DataSourceConfigurationSetter.ForTestWithCache(), new CurrentHttpContext());
			dataSource = dsFactory.Create(SetupFixtureForAssembly.Sql2005conf(ConnectionStringHelper.ConnectionStringUsedInTests, null), null);
			availableData = new AvailableData();
			availableData.ApplicationRole = new ApplicationRole{Name = "d"};
			businessUnit = new BusinessUnit("d");
			team = new Team{Description = new Description("k", "d")};
			site = new Site("d");
			site.AddTeam(team);
			businessUnit.AddSite(site);

			availableData.AddAvailableBusinessUnit(businessUnit);
			availableData.AddAvailableSite(site);
			availableData.AddAvailableTeam(team);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				new BusinessUnitRepository(uow).Add(businessUnit);
				new SiteRepository(uow).Add(site);
				new TeamRepository(uow).Add(team);
				new ApplicationRoleRepository(uow).Add(availableData.ApplicationRole);
				new AvailableDataRepository(uow).Add(availableData);
				uow.PersistAll();
			}

			//fill cache
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var avD = new AvailableDataRepository(uow).Get(availableData.Id.Value);
				LazyLoadingManager.Initialize(avD.AvailableBusinessUnits);
				LazyLoadingManager.Initialize(avD.AvailableTeams);
				LazyLoadingManager.Initialize(avD.AvailableSites);
			}
		}

		[TearDown]
		public void Teardown()
		{
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				new TeamRepository(uow).Remove(team);
				new SiteRepository(uow).Remove(site);
				new AvailableDataRepository(uow).Remove(availableData);
				new ApplicationRoleRepository(uow).Remove(availableData.ApplicationRole);
				new BusinessUnitRepository(uow).Remove(businessUnit);
				uow.PersistAll();
			}
		}
	}
}