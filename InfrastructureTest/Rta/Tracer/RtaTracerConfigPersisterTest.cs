using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Tracer
{
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerConfigPersisterTest : ISetup
	{
		public FakeDataSourceForTenant Tenants;
		public IRtaTracerConfigPersister Target;
		public ICurrentDataSource Tenant;
		public IDataSourceScope DataSource;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
		}

		[Test]
		public void ShouldUpdate()
		{
			Target.UpdateForTenant("usercode");

			Target.ReadAll().Single().UserCode.Should().Be("usercode");
		}

		[Test]
		public void ShouldUpdate2()
		{
			Target.UpdateForTenant("usercode1");
			Target.UpdateForTenant("usercode2");

			Target.ReadAll().Single().UserCode.Should().Be("usercode2");
		}

		[Test]
		public void ShouldUpdateForTenant()
		{
			Tenants.Has(new FakeDataSource {DataSourceName = "tenant1"});
			Tenants.Has(new FakeDataSource {DataSourceName = "tenant2"});

			using (DataSource.OnThisThreadUse("tenant1"))
				Target.UpdateForTenant("usercode1");
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.UpdateForTenant("usercode2");

			Target.ReadAll().Single(x => x.Tenant == "tenant1").UserCode.Should().Be("usercode1");
			Target.ReadAll().Single(x => x.Tenant == "tenant2").UserCode.Should().Be("usercode2");
		}

		[Test]
		public void ShouldDelete()
		{
			Target.UpdateForTenant("usercode");

			Target.DeleteForTenant();

			Target.ReadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteForTenant()
		{
			Tenants.Has(new FakeDataSource {DataSourceName = "tenant1"});
			Tenants.Has(new FakeDataSource {DataSourceName = "tenant2"});

			using (DataSource.OnThisThreadUse("tenant1"))
				Target.UpdateForTenant("usercode1");
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.DeleteForTenant();

			Target.ReadAll().Single().Tenant.Should().Be("tenant1");
		}
	}
}