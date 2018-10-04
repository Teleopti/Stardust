using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ToggleAdmin;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.InfrastructureTest.Toggle.Admin
{
	[DatabaseTestAttribute]
	public class FetchAllToggleOverridesTest
	{
		public IPersistToggleOverride PersistToggleOverride;
		public IFetchAllToggleOverrides Target;

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldReadSavedToggleOverride(bool value)
		{
			PersistToggleOverride.Save(Toggles.TestToggle, value);
			
			var overriden = Target.OverridenValues().Single();
			
			overriden.Key.Should().Be.EqualTo(Toggles.TestToggle.ToString());
			overriden.Value.Should().Be.EqualTo(value);
		}
	}
}