﻿using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Web.Areas.Toggle;

namespace Teleopti.Ccc.WebTest.Areas.Toggle
{
	public class ToggleHandlerControllerTest
	{
		[Test]
		public void ShouldBeEnabled()
		{
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var target = new ToggleHandlerController(toggleManager);
			toggleManager.Expect(x => x.IsEnabled(Toggles.EnabledFeature)).Return(true);

			target.IsEnabled(Toggles.EnabledFeature)
				.Should().Be.True();
		}

		[Test]
		public void ShouldBeDisabled()
		{
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var target = new ToggleHandlerController(toggleManager);
			toggleManager.Expect(x => x.IsEnabled(Toggles.EnabledFeature)).Return(false);

			target.IsEnabled(Toggles.EnabledFeature)
				.Should().Be.False();
		}
	}
}