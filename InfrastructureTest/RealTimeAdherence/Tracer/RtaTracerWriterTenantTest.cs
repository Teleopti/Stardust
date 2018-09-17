using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Tracer
{
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerWriterTenantTest : IIsolateSystem
	{
		public IRtaTracerWriter Target;
		public IRtaTracerReader Reader;
		public ICurrentDataSource Tenant;
		public IDataSourceScope DataSource;
		public FakeDataSourceForTenant DataSources;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
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