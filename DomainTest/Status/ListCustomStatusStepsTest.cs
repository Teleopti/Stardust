using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Status
{
	[DomainTest]
	public class ListCustomStatusStepsTest
	{
		public ListCustomStatusSteps Target;
		public FakeFetchCustomStatusSteps FetchCustomStatusSteps;
		public FakeConfigReader ConfigReader;
		
		[Test]
		public void ShouldReturnSimplePrimitives()
		{
			var statusStep = new CustomStatusStep(14, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), TimeSpan.Zero, TimeSpan.Zero, true);
			FetchCustomStatusSteps.Has(statusStep);

			var res = Target.Execute().Single();
			res.Id.Should().Be.EqualTo(statusStep.Id);
			res.Name.Should().Be.EqualTo(statusStep.Name);
			res.Description.Should().Be.EqualTo(statusStep.Description);
			res.CanBeDeleted.Should().Be.EqualTo(statusStep.CanBeDeleted);
		}
		
		[Test]
		public void ShouldIncludePingUrlIfCustomStatusStep()
		{
			ConfigReader.FakeSetting("StatusBaseUrl", "http://www.something.com/virtDir");
			var stepName = Guid.NewGuid().ToString();
			var statusStep = new CustomStatusStep(0, stepName, string.Empty, TimeSpan.Zero, TimeSpan.Zero, false);
			FetchCustomStatusSteps.Has(statusStep);
			
			Target.Execute().Single().PingUrl
				.Should().Be.EqualTo(ConfigReader.AppConfig("StatusBaseUrl") + "/ping/" + stepName);
		}

		[Test]
		public void ShouldReturnLimitAsSeconds()
		{
			var statusStep = new CustomStatusStep(0, string.Empty, string.Empty, TimeSpan.Zero, TimeSpan.FromMinutes(2), false);
			FetchCustomStatusSteps.Has(statusStep);
			
			Target.Execute().Single().Limit
				.Should().Be.EqualTo(2 * 60);
		}
	}
}