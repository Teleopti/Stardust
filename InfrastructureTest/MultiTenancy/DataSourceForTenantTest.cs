using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public class DataSourceForTenantTest
	{
		[Test]
		public void MakeSureDataSourceExistsShouldAddIfNotExist()
		{
			var dataSourcesFactory = MockRepository.GenerateMock<IDataSourcesFactory>();
			var dataSourceName = RandomName.Make();
			var appNhibConf = new Dictionary<string, string>();
			var statisticConnString = RandomName.Make();
			var applicationConnectionString = RandomName.Make();
			var dataSource = new FakeDataSource();
			dataSourcesFactory.Expect(x => x.Create(dataSourceName, applicationConnectionString, statisticConnString, appNhibConf)).Return(dataSource);
			var target = new DataSourceForTenant(dataSourcesFactory, new SetNoLicenseActivator(), new FindTenantByNameWithEnsuredTransactionFake());
			target.Tenant(dataSourceName).Should().Be.EqualTo(null);
			target.MakeSureDataSourceCreated(dataSourceName, applicationConnectionString, statisticConnString, appNhibConf);
			target.Tenant(dataSourceName).Should().Be.SameInstanceAs(dataSource);
		}

		[Test]
		public void MakeSureDataSourceExistsShouldNotAddIfExist()
		{
			var dataSourcesFactory = MockRepository.GenerateMock<IDataSourcesFactory>();
			var dataSourceName = RandomName.Make();
			var appNhibConf = new Dictionary<string, string>();
			var statisticConnString = RandomName.Make();
			var dataSource = new FakeDataSource { DataSourceName = dataSourceName };
			var target = new FakeDataSourceForTenant(dataSourcesFactory, null, null);
			target.Has(dataSource);

			target.Tenant(dataSourceName).Should().Be.SameInstanceAs(dataSource);
			dataSourcesFactory.AssertWasNotCalled(x => x.Create(appNhibConf, statisticConnString));
		}

		[Test]
		public void CanFindDataSourceByTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new FakeDataSourceForTenant(null, null, null);
			target.Has(ds);
			target.Tenant(dsName).Should().Be.SameInstanceAs(ds);
		}

		[Test]
		public void MissingDataSourceTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new FakeDataSourceForTenant(null, null, new FindTenantByNameWithEnsuredTransactionFake());
			target.Has(ds);
			target.Tenant("something else").Should().Be.Null();
		}

		[Test]
		public void NoDataSourceTenant()
		{
			var target = new DataSourceForTenant(null, null, new FindTenantByNameWithEnsuredTransactionFake());
			target.Tenant("something").Should().Be.Null();
		}

		[Test]
		public void CanRemoveTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new FakeDataSourceForTenant(null, null, new FindTenantByNameWithEnsuredTransactionFake());
			target.Has(ds);
			target.RemoveDataSource(dsName);
			target.Tenant(dsName).Should().Be.Null();
		}

		[Test]
		public void RemovingNonExistingTenantShouldNotThrow()
		{
			var target = new DataSourceForTenant(null, null, null);
			Assert.DoesNotThrow(() =>
			{
				target.RemoveDataSource(RandomName.Make());
			});
		}

		[Test]
		public void DoActionOnAllTenants()
		{
			var ds1 = MockRepository.GenerateMock<IDataSource>();
			ds1.Expect(x => x.DataSourceName).Return(RandomName.Make());
			var ds2 = MockRepository.GenerateMock<IDataSource>();
			ds2.Expect(x => x.DataSourceName).Return(RandomName.Make());
			var target = new FakeDataSourceForTenant(null, null, null);
			target.Has(ds1);
			target.Has(ds2);

			target.DoOnAllTenants_AvoidUsingThis(tenant => tenant.RemoveAnalytics());

			ds1.AssertWasCalled(x => x.RemoveAnalytics());
			ds2.AssertWasCalled(x => x.RemoveAnalytics());
		}

		[Test]
		public void ShouldCallSetLicenseActivatorWhenNewTenantIsAdded()
		{
			var dataSourcesFactory = MockRepository.GenerateMock<IDataSourcesFactory>();
			var dataSource = new FakeDataSource();
			dataSourcesFactory.Expect(x => x.Create(null, null, null,null)).IgnoreArguments().Return(dataSource);
			var setLicenseActivator = MockRepository.GenerateMock<ISetLicenseActivator>();

			var target = new DataSourceForTenant(dataSourcesFactory, setLicenseActivator, null);
			target.MakeSureDataSourceCreated(RandomName.Make(), null, null, new Dictionary<string, string>());

			setLicenseActivator.AssertWasCalled(x => x.Execute(dataSource));
		}

		[Test]
		public void ShouldFetchTenantFromDataSourceIfNotExists()
		{
			var existingTenant = new Tenant(RandomName.Make());
			var createdDataSource = new FakeDataSource();

			var findTenantByName = new FindTenantByNameWithEnsuredTransactionFake();
			findTenantByName.Has(existingTenant);
			var dataSourcesFactory = MockRepository.GenerateMock<IDataSourcesFactory>();
			dataSourcesFactory.Expect(
				x =>
					x.Create(existingTenant.Name, existingTenant.DataSourceConfiguration.ApplicationConnectionString,
						existingTenant.DataSourceConfiguration.AnalyticsConnectionString,
						existingTenant.ApplicationConfig)).Return(createdDataSource);

			var target = new DataSourceForTenant(dataSourcesFactory, new SetNoLicenseActivator(), findTenantByName);

			target.Tenant(existingTenant.Name)
				.Should().Be.SameInstanceAs(createdDataSource);
		}
}
}