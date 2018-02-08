﻿using System.IO;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		public static IDataSource DataSource;

		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			DataSource = DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			ServiceLocatorForLegacy.NestedUnitOfWorkStrategy = new SirLeakAlot();

			var personThatCreatesTestData = PersonFactory.CreatePerson("UserThatCreatesTestData", "password");

			TestState.BusinessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			TestState.BusinessUnit.Name = "BusinessUnit";

			StateHolderProxyHelper.SetupFakeState(DataSource, personThatCreatesTestData, TestState.BusinessUnit);
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(UnitOfWorkFactory.Current.ConnectionString);
			
			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				var testDataFactory = new TestDataFactory(new ThisUnitOfWork(uow), tenantUnitOfWorkManager, tenantUnitOfWorkManager, new LegacySetupResolver());
				testDataFactory.Apply(new PersonThatCreatesTestData(personThatCreatesTestData));
				testDataFactory.Apply(new DefaultLicense());
				testDataFactory.Apply(new BusinessUnitFromFakeState(TestState.BusinessUnit));
			}

			DataSourceHelper.BackupApplicationDatabase(123);
		}

		private static void disposeUnitOfWork()
		{
			TestState.UnitOfWork.Dispose();
			TestState.UnitOfWork = null;
		}

		private static void openUnitOfWork()
		{
			TestState.UnitOfWork = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork();
		}

		public static void BeginTest()
		{
			DataSourceHelper.RestoreApplicationDatabase(123);
			DataSourceHelper.ClearAnalyticsData();

			openUnitOfWork();
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(UnitOfWorkFactory.Current.ConnectionString);
			TestState.TestDataFactory = new TestDataFactory(new ThisUnitOfWork(TestState.UnitOfWork), tenantUnitOfWorkManager, tenantUnitOfWorkManager, new LegacySetupResolver());
			ServiceLocatorForLegacy.NestedUnitOfWorkStrategy = new SirLeakAlot();
		}

		public static void EndTest()
		{
			disposeUnitOfWork();
		}
	}
}
