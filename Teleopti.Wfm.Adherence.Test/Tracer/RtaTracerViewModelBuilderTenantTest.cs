using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.Test.Tracer
{
	[DomainTest]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerViewModelBuilderTenantTest
	{
		public RtaTracerViewModelBuilder Target;
		public FakeRtaTracerPersister RtaTracers;
		public IDataSourceScope Tenant;
		public FakeTenants Tenants;

		[Test]
		public void ShouldContainTenant()
		{
			Tenants.Has("tenant");
			using (Tenant.OnThisThreadUse("tenant"))
			{
				RtaTracers
					.Has(new RtaTracerLog<ProcessReceivedLog> {Tenant = "tenant"})
					.Has(new RtaTracerLog<ProcessEnqueuingLog> {Tenant = "tenant"})
					.Has(new RtaTracerLog<ProcessProcessingLog> {Tenant = "tenant"})
					.Has(new RtaTracerLog<ProcessActivityCheckLog> {Tenant = "tenant"})
					.Has(new RtaTracerLog<ProcessExceptionLog> {Tenant = "tenant"})
					;

				Target.Build().Tracers.Single().DataReceived.Single().Tenant.Should().Be("tenant");
				Target.Build().Tracers.Single().DataEnqueuing.Single().Tenant.Should().Be("tenant");
				Target.Build().Tracers.Single().DataProcessing.Single().Tenant.Should().Be("tenant");
				Target.Build().Tracers.Single().ActivityCheck.Single().Tenant.Should().Be("tenant");
				Target.Build().Tracers.Single().Exceptions.Single().Tenant.Should().Be("tenant");
			}
		}

		[Test]
		public void ShouldNotContainTenant()
		{
			using (Tenant.OnThisThreadUse(null as string))
			{
				RtaTracers
					.Has(new RtaTracerLog<ProcessReceivedLog> {Tenant = null})
					.Has(new RtaTracerLog<ProcessEnqueuingLog> {Tenant = null})
					.Has(new RtaTracerLog<ProcessProcessingLog> {Tenant = null})
					.Has(new RtaTracerLog<ProcessActivityCheckLog> {Tenant = null})
					.Has(new RtaTracerLog<ProcessExceptionLog> {Tenant = null})
					;

				Target.Build().Tracers.Single().DataReceived.Single().Tenant.Should().Be(null);
				Target.Build().Tracers.Single().DataEnqueuing.Single().Tenant.Should().Be(null);
				Target.Build().Tracers.Single().DataProcessing.Single().Tenant.Should().Be(null);
				Target.Build().Tracers.Single().ActivityCheck.Single().Tenant.Should().Be(null);
				Target.Build().Tracers.Single().Exceptions.Single().Tenant.Should().Be(null);
			}
		}
	}
}