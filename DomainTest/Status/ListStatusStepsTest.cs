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
		public FakeFetchCustomStatusSteps FetchCustomStatusSteps;
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

		[Test]
		public void ShouldReturnCustomStatusSteps()
		{
			var stepName = Guid.NewGuid().ToString();
			var description = Guid.NewGuid().ToString();
			var statusStep = new CustomStatusStep(stepName, description, null, TimeSpan.Zero);
			FetchCustomStatusSteps.Has(statusStep);

			Target.Execute(uri, stepName).Single(x => x.Name == statusStep.Name).Description
				.Should().Be.EqualTo(description);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStatusStep>().For<IStatusStep>();
			isolate.UseTestDouble<FakeFetchCustomStatusSteps>().For<IFetchCustomStatusSteps>();
		}
	}
}