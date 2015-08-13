using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Config
{
	public class ReadDataSourceConfigurationTest
	{
		[Test]
		public void ShouldGetDataSourceConfiguration()
		{
			var tenant = new Tenant(RandomName.Make());
			tenant.DataSourceConfiguration.SetAnalyticsConnectionString(string.Format("Initial Catalog={0}", RandomName.Make()));
			tenant.DataSourceConfiguration.SetApplicationConnectionString(string.Format("Initial Catalog={0}", RandomName.Make()));
			tenant.DataSourceConfiguration.ApplicationNHibernateConfig["test"] = RandomName.Make();
			var loadAllTenants = MockRepository.GenerateMock<ILoadAllTenants>();
			loadAllTenants.Expect(x => x.Tenants()).Return(new[] { tenant });

			var target = new ReadDataSourceConfiguration(loadAllTenants);
			var result = target.Read().Single();

			result.Key.Should().Be.EqualTo(tenant.Name);
			result.Value.ApplicationConnectionString.Should().Be.EqualTo(tenant.DataSourceConfiguration.ApplicationConnectionString);
			result.Value.AnalyticsConnectionString.Should().Be.EqualTo(tenant.DataSourceConfiguration.AnalyticsConnectionString);
			result.Value.ApplicationNHibernateConfig.Should().Have.SameValuesAs(tenant.DataSourceConfiguration.ApplicationNHibernateConfig);
		}

		[Test]
		public void ShouldGetAll()
		{
			var loadAllTenants = MockRepository.GenerateMock<ILoadAllTenants>();
			loadAllTenants.Expect(x => x.Tenants()).Return(new[] {new Tenant(RandomName.Make()), new Tenant(RandomName.Make())});

			var target = new ReadDataSourceConfiguration(loadAllTenants);
			target.Read().Count
				.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldNotUseSameDictionaryInstance()
		{
			var tenant = new Tenant(RandomName.Make());
			tenant.DataSourceConfiguration.ApplicationNHibernateConfig["something"] = "something";
			var loadAllTenants = MockRepository.GenerateMock<ILoadAllTenants>();
			loadAllTenants.Expect(x => x.Tenants()).Return(new[] { tenant });

			var target = new ReadDataSourceConfiguration(loadAllTenants);
			var result = target.Read().Single();

			result.Value.ApplicationNHibernateConfig
				.Should().Not.Be.SameInstanceAs(tenant.DataSourceConfiguration.ApplicationNHibernateConfig);
		}
	}
}