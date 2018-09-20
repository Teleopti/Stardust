using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ToggleAdmin;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ToggleAdmin
{
	[DomainTest]
	public class AllToggleNamesWithoutOverridesTest
	{
		public AllToggleNamesWithoutOverrides Target;
		public IAllToggles AllToggles;
		public FakeFetchAllToggleOverrides FetchAllToggleOverrides;
		
		[Test]
		public void ShouldShowAllTogglesIfNoOverride()
		{
			var result = Target.Execute();

			result.Should().Have.SameValuesAs(AllToggles.Toggles().Select(x => x.ToString()));
		}

		[Test]
		public void ShouldSkipOverridenToggleName()
		{
			FetchAllToggleOverrides.Add(Toggles.TestToggle);
			
			var result = Target.Execute();

			result.Should().Not.Contain(Toggles.TestToggle.ToString());
		}

		[Test]
		public void ShouldSortTogglesByName()
		{
			var result = Target.Execute();

			result.Should().Have.SameSequenceAs(AllToggles.Toggles().Select(x => x.ToString()).OrderBy(x => x));
		}
	}
}