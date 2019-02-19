using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Status
{
	[DomainTest]
	public class ListStatusStepsTest : IIsolateSystem
	{
		public ListStatusSteps Target;
		public FakeStatusStep FakeStatusStep;
		
		[Test]
		public void ShouldListMonitorStepNames()
		{
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute(string.Empty).Select(x => x.Name).Should().Contain(FakeStatusStep.Name);
		}

		[Test]
		public void ShouldIncludeUrl()
		{
			var baseUrl = Guid.NewGuid().ToString();
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute(baseUrl).Single(x => x.Name == FakeStatusStep.Name).Url.Should().Be.EqualTo(baseUrl + "/" + FakeStatusStep.Name);
		}
		
		[Test]
		public void ShouldIncludeUrl_IfBaseUrlEndsWithSlash()
		{
			var baseUrl = Guid.NewGuid() + "/";
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute(baseUrl).Single(x => x.Name == FakeStatusStep.Name).Url.Should().Be.EqualTo(baseUrl + FakeStatusStep.Name);
		}

		[Test]
		public void ShouldIncludeDescription()
		{
			FakeStatusStep.Description = Guid.NewGuid().ToString();
		
			Target.Execute(string.Empty).Single(x => x.Name == FakeStatusStep.Name).Description.Should().Be.EqualTo(FakeStatusStep.Description);
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStatusStep>().For<IStatusStep>();
		}
	}
}