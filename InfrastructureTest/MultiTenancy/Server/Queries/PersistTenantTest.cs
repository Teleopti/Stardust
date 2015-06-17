using System;
using System.Collections.Generic;
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
			tenant.SetApplicationConnectionString(appConnString);
			tenant.SetAnalyticsConnectionString(analConnString);
			target.Persist(tenant);

			var result = tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>();

			result.ApplicationConnectionString.Should().Be.EqualTo(appConnString);
			result.AnalyticsConnectionString.Should().Be.EqualTo(analConnString);
		}

		[Test]
		public void ShouldPersistApplicationNhibernateConfig()
		{
			var tenant = new Tenant(RandomName.Make());
			var key1 = RandomName.Make();
			var value1 = RandomName.Make();
			var key2 = RandomName.Make();
			var value2 = RandomName.Make();
			tenant.ApplicationNHibernateConfig = new Dictionary<string, string>
			{
				{key1, value1},
				{key2, value2}
			};

			target.Persist(tenant);
			tenantUnitOfWorkManager.CurrentSession().Flush();
			var result = tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>();

			result.ApplicationNHibernateConfig
				.Should()
				.Have.SameValuesAs(new KeyValuePair<string, string>(key1, value1), new KeyValuePair<string, string>(key2, value2));
		}

		[Test]
		public void InvalidApplicationConnectionString()
		{
			var tenant = new Tenant(RandomName.Make());
			Assert.Throws<ArgumentException>(() =>
				tenant.SetApplicationConnectionString(RandomName.Make()));
		}

		[Test]
		public void InvalidAnalyticsConnectionString()
		{
			var tenant = new Tenant(RandomName.Make());
			Assert.Throws<ArgumentException>(() =>
				tenant.SetAnalyticsConnectionString(RandomName.Make()));
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
			result.ApplicationNHibernateConfig.Single(cfg => cfg.Key == Environment.CommandTimeout).Value
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