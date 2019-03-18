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
	public class ListStatusStepsTest : IIsolateSystem
	{
		public ListStatusSteps Target;
		public FakeStatusStep FakeStatusStep;
		public FakeFetchCustomStatusSteps FetchCustomStatusSteps;
		public FakeConfigReader ConfigReader;
		
		[Test]
		public void ShouldListMonitorStepNames()
		{
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute().Select(x => x.Name).Should().Contain(FakeStatusStep.Name);
		}

		[Test]
		public void ShouldIncludeStatusUrl()
		{
			ConfigReader.FakeSetting("StatusBaseUrl", "http://www.something.com/virtDir");
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute().Single(x => x.Name == FakeStatusStep.Name).StatusUrl
				.Should().Be.EqualTo(ConfigReader.AppConfig("StatusBaseUrl") + "/check/" + FakeStatusStep.Name);
		}

		[Test]
		public void ShouldIncludeDescription()
		{
			FakeStatusStep.Description = Guid.NewGuid().ToString();
		
			Target.Execute().Single(x => x.Name == FakeStatusStep.Name).Description
				.Should().Be.EqualTo(FakeStatusStep.Description);
		}

		[Test]
		public void ShouldReturnCustomStatusSteps()
		{
			var stepName = Guid.NewGuid().ToString();
			var description = Guid.NewGuid().ToString();
			var statusStep = new CustomStatusStep(0, stepName, description, TimeSpan.Zero, TimeSpan.Zero, false);
			FetchCustomStatusSteps.Has(statusStep);

			Target.Execute().Single(x => x.Name == statusStep.Name).Description
				.Should().Be.EqualTo(description);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStatusStep>().For<IStatusStep>();
		}
	}
}