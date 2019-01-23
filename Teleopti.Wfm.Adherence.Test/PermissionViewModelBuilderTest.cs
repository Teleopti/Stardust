using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test
{
	[DomainTest]
	[TestFixture]
	[FakePermissions]
	public class PermissionViewModelBuilderTest
	{
		public PermissionsViewModelBuilder Target;
		public FakePermissions Permissions;

		[Test]
		public void ShouldHaveHistoricalOverviewPermission()
		{
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.HistoricalOverview);

			Target.Build(null, null)
				.HistoricalOverview.Should().Be.True();
		}

		[Test]
		public void ShouldNotHaveHistoricalOverviewPermission()
		{
			Target.Build(null, null)
				.HistoricalOverview.Should().Be.False();
		}
		
		[Test]
		public void ShouldHaveModifySkillGroupPermission()
		{
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup);

			Target.Build(null, null)
				.ModifySkillGroup.Should().Be.True();
		}	
		
		[Test]
		public void ShouldNotHaveModifySkillGroupPermission()
		{
			Target.Build(null, null)
				.ModifySkillGroup.Should().Be.False();
		}
		
		[Test]
		public void ShouldHaveAdjustAdherencePermission()
		{
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.AdjustAdherence);

			Target.Build(null, null)
				.AdjustAdherence.Should().Be.True();
		}
		
		[Test]
		public void ShouldNotHaveAdjustAdherencePermission()
		{
			Target.Build(null, null)
				.AdjustAdherence.Should().Be.False();
		}
	}
}