using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public class AssertConfigurationTest
	{
		[Test]
		public void ShouldRunInToggleDevMode()
		{
			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			toggleQuerier.IsEnabled(Toggles.TestToggle).Should().Be.True();
		}
	}
}