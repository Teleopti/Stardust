using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	public class TogglesActiveTest
	{
		[Test]
		public void ShouldGetEnabledToggle()
		{
			const Toggles toggle = (Toggles) 33;
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var allToggles = MockRepository.GenerateMock<IAllToggles>();
			var target = new TogglesActive(toggleManager, allToggles);
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(true);
			allToggles.Stub(x => x.Toggles()).Return(new HashSet<Toggles>(new[] {toggle}));

			target.AllActiveToggles()[toggle]
				.Should().Be.True();
		}

		[Test]
		public void ShouldGetDisabledToggle()
		{
			const Toggles toggle = (Toggles)76;
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var allToggles = MockRepository.GenerateMock<IAllToggles>();
			var target = new TogglesActive(toggleManager, allToggles);
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(false);
			allToggles.Stub(x => x.Toggles()).Return(new HashSet<Toggles>(new[] { toggle }));

			target.AllActiveToggles()[toggle]
				.Should().Be.False();
		}

		[Test]
		public void ShouldGetMultipleToggles()
		{
			const Toggles toggle1 = (Toggles)1;
			const Toggles toggle2 = (Toggles)2;
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var allToggles = MockRepository.GenerateMock<IAllToggles>();
			var target = new TogglesActive(toggleManager, allToggles);
			toggleManager.Stub(x => x.IsEnabled(toggle1)).Return(true);
			toggleManager.Stub(x => x.IsEnabled(toggle2)).Return(false);
			allToggles.Stub(x => x.Toggles()).Return(new HashSet<Toggles>(new[] { toggle1, toggle2 }));

			target.AllActiveToggles()
				.Should().Have.SameValuesAs(
					new KeyValuePair<Toggles, bool>(toggle1, true), new KeyValuePair<Toggles, bool>(toggle2, false)
				);
		}
	}
}