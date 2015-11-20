using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Toggle;

namespace Teleopti.Ccc.WebTest.Areas.Toggle
{
	public class ToggleHandlerControllerTest
	{
		[Test]
		public void ShouldBeEnabled()
		{
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var target = new ToggleHandlerController(toggleManager,null);
			toggleManager.Expect(x => x.IsEnabled(Toggles.TestToggle)).Return(true);

			((OkNegotiatedContentResult<ToggleEnabledResult>)target.IsEnabled(Toggles.TestToggle)).Content.IsEnabled
				.Should().Be.True();
		}

		[Test]
		public void ShouldBeDisabled()
		{
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var target = new ToggleHandlerController(toggleManager,null);
			toggleManager.Expect(x => x.IsEnabled(Toggles.TestToggle)).Return(false);

			((OkNegotiatedContentResult<ToggleEnabledResult>)target.IsEnabled(Toggles.TestToggle)).Content.IsEnabled
				.Should().Be.False();
		}
	}
}