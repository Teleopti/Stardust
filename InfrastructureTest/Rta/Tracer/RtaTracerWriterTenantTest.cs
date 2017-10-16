using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Tracer
{
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	public class RtaTracerWriterTenantTest
	{
		public IRtaTracerWriter Target;
		public IRtaTracerReader Reader;
		public ICurrentDataSource Tenant;

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
	}
}