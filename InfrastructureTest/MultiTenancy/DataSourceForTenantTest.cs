using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
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
			var dataSource = new FakeDataSource { DataSourceName = dataSourceName };
			dataSourcesFactory.Expect(x => x.Create(appNhibConf, statisticConnString)).Return(dataSource);
			var target = new DataSourceForTenant(dataSourcesFactory);
			target.Tenant(dataSourceName).Should().Be.EqualTo(null);
			target.MakeSureDataSourceExists(dataSourceName, RandomName.Make(), statisticConnString, appNhibConf);
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
			var target = new DataSourceForTenant(dataSourcesFactory);
			target.MakeSureDataSourceExists_UseOnlyFromTests(dataSource);

			target.Tenant(dataSourceName).Should().Be.SameInstanceAs(dataSource);
			dataSourcesFactory.AssertWasNotCalled(x => x.Create(appNhibConf, statisticConnString));
		}

		[Test]
		public void CanFindDataSourceByTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new DataSourceForTenant(null);
			target.MakeSureDataSourceExists_UseOnlyFromTests(ds);
			target.Tenant(dsName).Should().Be.SameInstanceAs(ds);
		}

		[Test]
		public void MissingDataSourceTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new DataSourceForTenant(null);
			target.MakeSureDataSourceExists_UseOnlyFromTests(ds);
			target.Tenant("something else").Should().Be.Null();
		}

		[Test]
		public void NoDataSourceTenant()
		{
			var target = new DataSourceForTenant(null);
			target.Tenant("something").Should().Be.Null();
		}

		[Test]
		public void DoActionOnAllTenants()
		{
			var ds1 = MockRepository.GenerateMock<IDataSource>();
			var ds2 = MockRepository.GenerateMock<IDataSource>();
			var target = new DataSourceForTenant(null);
			target.MakeSureDataSourceExists_UseOnlyFromTests(ds1);
			target.MakeSureDataSourceExists_UseOnlyFromTests(ds2);

			target.DoOnAllTenants_AvoidUsingThis(tenant => tenant.ResetStatistic());

			ds1.AssertWasCalled(x => x.ResetStatistic());
			ds2.AssertWasCalled(x => x.ResetStatistic());
		}
	}
}