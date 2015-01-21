using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Infrastructure.PerformanceTool;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Core;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Models;

namespace Teleopti.Ccc.WebTest.Areas.PerformanceTool.Controllers
{
	public class ApplicationControllerTest
	{
		[Test]
		public void ShouldReturnDefaultViewInDefaultAction()
		{
			var target = new ApplicationController(null,null,null,null, null);
			var result = target.Index();
			result.ViewName.Should().Be.Empty();
		}

		[Test]
		public void ShouldPrepareDataForManageAdherenceLoadTest()
		{
			var personGenerator = MockRepository.GenerateStub<IPersonGenerator>();
			var stateGenerator = MockRepository.GenerateStub<IStateGenerator>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new ApplicationController(performanceCounter, null, null, personGenerator, stateGenerator);
			target.ManageAdherenceLoadTest(1000);
			performanceCounter.AssertWasCalled(x=>x.ResetCount());
			personGenerator.AssertWasCalled(x => x.Generate(1000));
			stateGenerator.AssertWasCalled(x => x.Generate(1000));
		}
	}

}