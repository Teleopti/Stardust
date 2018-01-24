using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	public class RtaTracerWriterTenantTest : ISetup
	{
		public IRtaTracerWriter Target;
		public IRtaTracerReader Reader;
		public ICurrentDataSource Tenant;
		public IDataSourceScope DataSource;
		public FakeDataSourceForTenant DataSources;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
		}

		[Test]
		public void ShouldWriteTenant()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = Tenant.CurrentName()});

			Reader.ReadOfType<ProcessReceivedLog>()
				.First().Tenant.Should().Be(Tenant.CurrentName());
		}

		[Test]
		public void ShouldReadByCurrentTenant()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = "another tenant"});
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = Tenant.CurrentName()});

			Reader.ReadOfType<ProcessReceivedLog>()
				.First().Tenant.Should().Be(Tenant.CurrentName());
		}

		[Test]
		public void ShouldClearCurrentTenant()
		{
			DataSources.Has(new FakeDataSource {DataSourceName = "another tenant"});
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = "another tenant"});
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = Tenant.CurrentName()});

			Target.Clear();

			Reader.ReadOfType<ProcessReceivedLog>().Should().Be.Empty();
			using (DataSource.OnThisThreadUse("another tenant"))
				Reader.ReadOfType<ProcessReceivedLog>().Should().Not.Be.Empty();
		}
	}
}