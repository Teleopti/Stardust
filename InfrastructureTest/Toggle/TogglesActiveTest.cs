using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.InfrastructureTest.Toggle
{
	public class TogglesActiveTest
	{
		[Test]
		public void ShouldGetEnabledToggle()
		{
			const Toggles toggle = (Toggles) 33;
			var toggleManager = new FakeToggleManager(toggle);
			var allToggles = new FakeAllToggles(toggle);
			var target = new TogglesActive(toggleManager, allToggles);

			target.AllActiveToggles()[toggle]
				.Should().Be.True();
		}

		[Test]
		public void ShouldGetDisabledToggle()
		{
			const Toggles toggle = (Toggles)76;
			var toggleManager = new FakeToggleManager();
			var allToggles = new FakeAllToggles(toggle);
			var target = new TogglesActive(toggleManager, allToggles);

			target.AllActiveToggles()[toggle]
				.Should().Be.False();
		}

		[Test]
		public void ShouldGetMultipleToggles()
		{
			const Toggles toggle1 = (Toggles)1;
			const Toggles toggle2 = (Toggles)2;
			var toggleManager = new FakeToggleManager();
			var allToggles = new FakeAllToggles(toggle1, toggle2);
			var target = new TogglesActive(toggleManager, allToggles);
			toggleManager.Enable(toggle1);
			toggleManager.Disable(toggle2);

			target.AllActiveToggles()
				.Should().Have.SameValuesAs(
					new KeyValuePair<Toggles, bool>(toggle1, true), new KeyValuePair<Toggles, bool>(toggle2, false)
				);
		}
	}
}