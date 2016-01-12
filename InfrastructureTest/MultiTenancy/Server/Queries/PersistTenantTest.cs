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
			var aggConnString = string.Format("Data source={0};Initial Catalog={1}", RandomName.Make(), RandomName.Make());

			var tenant = new Tenant(RandomName.Make());
			tenant.DataSourceConfiguration.SetApplicationConnectionString(appConnString);
			tenant.DataSourceConfiguration.SetAnalyticsConnectionString(analConnString);
			tenant.DataSourceConfiguration.SetAggregationConnectionString(aggConnString);
			target.Persist(tenant);

			var result = tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>();

			result.DataSourceConfiguration.ApplicationConnectionString.Should().Be.EqualTo(appConnString);
			result.DataSourceConfiguration.AnalyticsConnectionString.Should().Be.EqualTo(analConnString);
			result.DataSourceConfiguration.AggregationConnectionString.Should().Be.EqualTo(aggConnString);
		}

		[Test]
		public void ShouldPersistApplicationNhibernateConfig()
		{
			var tenant = new Tenant(RandomName.Make());
			tenant.DataSourceConfiguration.SetNHibernateConfig(RandomName.Make(), RandomName.Make());
			target.Persist(tenant);
			tenantUnitOfWorkManager.CurrentSession().Flush();
			tenantUnitOfWorkManager.CurrentSession().Clear();
		
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

		[Test]
		public void ModifyingConfigRefShouldNotModifyPersistedData()
		{
			var tenant = new Tenant(RandomName.Make());
			var key = RandomName.Make();
			var value = RandomName.Make();
			tenant.DataSourceConfiguration.SetNHibernateConfig(key, value);
			tenant.DataSourceConfiguration.ApplicationNHibernateConfig[key] = RandomName.Make();

			tenant.DataSourceConfiguration.ApplicationNHibernateConfig[key]
				.Should().Be.EqualTo(value);
		}

		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(ConnectionStringHelper.ConnectionStringUsedInTests);
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			target = new PersistTenant(tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Cleanup()
		{
			tenantUnitOfWorkManager.Dispose();
		} 
	}
}