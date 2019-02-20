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
		private const string actionName = "action/Name";
		private static readonly Uri uri = new Uri("http://www.something.com");

		[Test]
		public void ShouldListMonitorStepNames()
		{
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute(uri, string.Empty).Select(x => x.Name).Should().Contain(FakeStatusStep.Name);
		}

		[Test]
		public void ShouldIncludeUrl()
		{
			var baseUrl = new Uri(uri, "virtualDir");
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute(baseUrl, actionName).Single(x => x.Name == FakeStatusStep.Name).Url.Should().Be.EqualTo(baseUrl + "/" + actionName + "/" + FakeStatusStep.Name);
		}
		
		[Test]
		public void ShouldIncludeUrl_IfBaseUrlEndsWithSlash()
		{
			var baseUrl = new Uri(uri, "/");
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute(baseUrl, actionName).Single(x => x.Name == FakeStatusStep.Name).Url.Should().Be.EqualTo(baseUrl + actionName + "/" + FakeStatusStep.Name);
		}

		[Test]
		public void ShouldIncludeDescription()
		{
			FakeStatusStep.Description = Guid.NewGuid().ToString();
		
			Target.Execute(uri, string.Empty).Single(x => x.Name == FakeStatusStep.Name).Description.Should().Be.EqualTo(FakeStatusStep.Description);
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStatusStep>().For<IStatusStep>();
		}
	}
}