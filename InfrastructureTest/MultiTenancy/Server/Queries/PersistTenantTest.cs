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
			var appConnString = $"Data source={RandomName.Make()};Initial Catalog={RandomName.Make()}";
			var analConnString = $"Data source={RandomName.Make()};Initial Catalog={RandomName.Make()}";
			var aggConnString = $"Data source={RandomName.Make()};Initial Catalog={RandomName.Make()}";

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
			tenant.SetApplicationConfig(RandomName.Make(), RandomName.Make());
			target.Persist(tenant);
			
			var result = tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>();

			result.ApplicationConfig
				.Should()
				.Have.SameValuesAs(tenant.ApplicationConfig);
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
			result.ApplicationConfig.Single(cfg => cfg.Key == Environment.CommandTimeout).Value
				.Should().Be.EqualTo("60");
		}

		[Test]
		public void ModifyingConfigRefShouldNotModifyPersistedData()
		{
			var tenant = new Tenant(RandomName.Make());
			var key = RandomName.Make();
			var value = RandomName.Make();
			tenant.SetApplicationConfig(key, value);
			tenant.ApplicationConfig[key] = RandomName.Make();

			tenant.ApplicationConfig[key]
				.Should().Be.EqualTo(value);
		}

		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ApplicationConnectionString());
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