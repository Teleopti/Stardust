using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ToggleAdmin;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ToggleAdmin
{
	[DomainTest]
	public class DeleteToggleOverrideTest
	{
		public DeleteToggleOverride Target;
		public FakePersistToggleOverride PersistToggleOverride;
		
		[Test]
		public void ShouldDeleteToggle()
		{
			const Toggles toggle = Toggles.TestToggle;
			
			Target.Execute(toggle.ToString());

			PersistToggleOverride.DeletedToggles.Should()
				.Have.SameValuesAs(Toggles.TestToggle.ToString());
		}

		[Test]
		public void ShouldAlsoDeleteNonExistingToggle()
		{
			var nonExistingToggle = Guid.NewGuid().ToString();
			
			Target.Execute(nonExistingToggle);

			PersistToggleOverride.DeletedToggles.Should()
				.Have.SameValuesAs(nonExistingToggle);
		}
	}
}