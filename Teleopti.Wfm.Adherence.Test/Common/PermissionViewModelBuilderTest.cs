using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;

namespace Teleopti.Wfm.Adherence.Test.Common
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
	}
}