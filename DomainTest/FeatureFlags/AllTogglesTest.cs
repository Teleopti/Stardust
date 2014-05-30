using System.Collections.Generic;
using System.Linq;
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
				.Should().Contain(Toggles.TestToggle);
		}

		[Test]
		public void ShouldBeNamed_Module_Name_Pbi()
		{

			foreach (var toggle in allTogglesThatShouldFollowingNamingConventions())
			{
				Assert.That(namedModuleNamePbiSeparatedByUnderscore(toggle), "Wrong naming for featuretoggle " + toggle + ", naming should be Area_Name_PBI");
			}
		}

		[Test]
		public void ShouldEndWithPbiNumber()
		{
			foreach (var toggle in allTogglesThatShouldFollowingNamingConventions())
			{
				Assert.That(endsWithPbiNumber(toggle), "Wrong naming for featuretoggle " + toggle + ", last value should be the PBInumber");
			}
		}

		private static IEnumerable<Toggles> allTogglesThatShouldFollowingNamingConventions()
		{
			return new AllToggles().Toggles().Except(new[] {Toggles.TestToggle});
		}

		private static bool namedModuleNamePbiSeparatedByUnderscore(Toggles toggle) 
		{
			return toggle.ToString().Split('_').Count().Equals(3);
		}

		private static bool endsWithPbiNumber(Toggles toggle) 
		{
			var parts = toggle.ToString().Split('_');
				if (parts.Last().Any(c => c < '0' || c > '9'))
				{
					return false;
				}
				return parts.Last().Length==5;
		}

	}
}