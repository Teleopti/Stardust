using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.Test.Tracer
{
	[DomainTest]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerNullTest
	{
		public IRtaTracer Target;
		public FakeRtaTracerPersister Logs;

		[Test]
		public void ShouldNotThrowOnNullUserCode()
		{
			Target.Trace(null);

			Assert.DoesNotThrow(() =>
			{
				Target.StateReceived("usercode", "statecode");
			});
		}
		
		[Test]
		public void ShouldNotMatchNullUserCodeWithEmpty()
		{
			Target.Trace(null);

			Target.StateReceived("", "statecode");

			Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}
		
		[Test]
		public void ShouldNotThrowOnNullPersonMapping()
		{
			Target.Trace("usercode");

			Assert.DoesNotThrow(() =>
			{
				Target.ActivityCheck(Guid.NewGuid());
			});
		}
	}
	
}