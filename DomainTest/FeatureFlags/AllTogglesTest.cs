using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.DomainTest.FeatureFlags
{
	public class AllTogglesTest
	{
		[Test]
		public void ShouldContainTestToggle()
		{
			var target = new AllToggles();
			target.Toggles()
				.Should().Contain(Toggles.EnabledFeature);
		}
	}
}