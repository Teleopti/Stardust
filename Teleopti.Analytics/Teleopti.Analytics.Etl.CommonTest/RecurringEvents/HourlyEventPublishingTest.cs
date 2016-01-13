using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Analytics.Etl.CommonTest.RecurringEvents
{
	[DomainTest]
	public class HourlyEventPublishingTest : ISetup
	{
		public RecurringEventPublisher Target;
		public FakeHangfireEventClient Hangfire;
		public FakeTenants Tenants;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new EtlModule(configuration));
			system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();
			system.UseTestDouble<FakeBaseConfigurationRepository>().For<IBaseConfigurationRepository>();
		}

		[Test]
		public void ShouldAddRecurringEventPublishing()
		{
			Tenants.Has(new Tenant("t"));

			Target.Tick();

			Hangfire.AddedRecurring.Should().Be.True();
		}

		[Test]
		public void ShouldAddWithTenant()
		{
			var tenant = RandomName.Make("tenant");
			Tenants.Has(new Tenant(tenant));

			Target.Tick();

			Hangfire.RecurringTenant.Should().Be(tenant);
		}
	}

	public class FakeBaseConfigurationRepository : IBaseConfigurationRepository
	{
		public IBaseConfiguration LoadBaseConfiguration(string connectionString)
		{
			return new BaseConfiguration(null, null, null, false);
		}

		public void SaveBaseConfiguration(string connectionString, IBaseConfiguration configuration)
		{
			throw new NotImplementedException();
		}
	}
}
