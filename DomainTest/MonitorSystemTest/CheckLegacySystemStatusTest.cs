using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	[MonitorTest]
	public class CheckLegacySystemStatusTest
	{
		public CheckLegacySystemStatus Target;
		public FakeCallLegacySystemStatus CallLegacySystemStatus;
		
		[Test]
		public void ShouldReturnOk()
		{
			Target.Execute()
				.Should().Be.EqualTo(HttpStatusCode.OK);
		}

		[Test]
		public void ShouldReturnIfFailure()
		{
			const HttpStatusCode failure = HttpStatusCode.InternalServerError;
			CallLegacySystemStatus.SetReturnValue(failure);

			Target.Execute()
				.Should().Be.EqualTo(failure);
		}
	}
}