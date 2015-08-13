using System;
using System.Linq;
using NHibernate.Exceptions;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class PersistTenantTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;
		private PersistTenant target;

		[Test]
		public void ShouldPersistTenant()
		{
			var tenant = new Tenant(RandomName.Make());

			target.Persist(tenant);

			tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void NameMustBeUnique()
		{
			var name = RandomName.Make();
			var tenant1 = new Tenant(name);
			var tenant2 = new Tenant(name);
			target.Persist(tenant1);
			Assert.Throws<GenericADOException>(() => target.Persist(tenant2));
		}

		[Test]
		public void ShouldPersistConnectionStrings()
		{
			var appConnString = string.Format("Data source={0};Initial Catalog={1}", RandomName.Make(), RandomName.Make());
			var analConnString = string.Format("Data source={0};Initial Catalog={1}", RandomName.Make(), RandomName.Make());

			var tenant = new Tenant(RandomName.Make());
			tenant.DataSourceConfiguration.SetApplicationConnectionString(appConnString);
			tenant.DataSourceConfiguration.SetAnalyticsConnectionString(analConnString);
			target.Persist(tenant);

			var result = tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>();

			result.DataSourceConfiguration.ApplicationConnectionString.Should().Be.EqualTo(appConnString);
			result.DataSourceConfiguration.AnalyticsConnectionString.Should().Be.EqualTo(analConnString);
		}

		[Test]
		public void ShouldPersistApplicationNhibernateConfig()
		{
			var tenant = new Tenant(RandomName.Make());
			var key1 = RandomName.Make();
			var value1 = RandomName.Make();
			var key2 = RandomName.Make();
			var value2 = RandomName.Make();
			tenant.DataSourceConfiguration.ApplicationNHibernateConfig[key1] = value1;
			tenant.DataSourceConfiguration.ApplicationNHibernateConfig[key2] = value2;

			target.Persist(tenant);
			tenantUnitOfWorkManager.CurrentSession().Flush();
			var result = tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>();

			result.DataSourceConfiguration.ApplicationNHibernateConfig
				.Should()
				.Have.SameValuesAs(tenant.DataSourceConfiguration.ApplicationNHibernateConfig);
		}

		[Test]
		public void InvalidApplicationConnectionString()
		{
			var tenant = new Tenant(RandomName.Make());
			Assert.Throws<ArgumentException>(() =>
				tenant.DataSourceConfiguration.SetApplicationConnectionString(RandomName.Make()));
		}

		[Test]
		public void InvalidAnalyticsConnectionString()
		{
			var tenant = new Tenant(RandomName.Make());
			Assert.Throws<ArgumentException>(() =>
				tenant.DataSourceConfiguration.SetAnalyticsConnectionString(RandomName.Make()));
		}

		[Test]
		public void ShouldSetCommandTimeoutWithDefaultValueToFollowOldNhibFile()
		{
			//personally I think this could be set in code instead... (and be overriden in entity if needed). But I do as Anders said.
			var tenant = new Tenant(RandomName.Make());
			target.Persist(tenant);
			tenantUnitOfWorkManager.CurrentSession().Flush();
			var result = tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>();
			result.DataSourceConfiguration.ApplicationNHibernateConfig.Single(cfg => cfg.Key == Environment.CommandTimeout).Value
				.Should().Be.EqualTo("60");
		}

		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			tenantUnitOfWorkManager.Start();
			target = new PersistTenant(tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Cleanup()
		{
			tenantUnitOfWorkManager.Dispose();
		} 
	}
}