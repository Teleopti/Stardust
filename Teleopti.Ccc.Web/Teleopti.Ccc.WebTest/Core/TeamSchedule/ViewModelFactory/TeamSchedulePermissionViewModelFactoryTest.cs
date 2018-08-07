using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	[TestFixture]
	[DomainTest]
	[FakePermissions]
	public class TeamSchedulePermissionViewModelFactoryTest : IIsolateSystem
	{
		public TeamSchedulePermissionViewModelFactory Target;
		public FakePermissions FakePermissions;
		public ILoggedOnUser LoggedOnUser;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<TeamSchedulePermissionViewModelFactory>().For<ITeamSchedulePermissionViewModelFactory>();
		}

		[Test]
		public void ShouldWithShiftTradeBulletinBoardPermission()
		{
			var result = Target.CreateTeamSchedulePermissionViewModel();
			result.ShiftTradeBulletinBoardPermission.Should().Be.False();

			FakePermissions.HasPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);

			result = Target.CreateTeamSchedulePermissionViewModel();
			result.ShiftTradeBulletinBoardPermission.Should().Be.True();
		}

		[Test]
		public void ShouldWithShiftTradePermisssion()
		{
			var result = Target.CreateTeamSchedulePermissionViewModel();
			result.ShiftTradePermisssion.Should().Be.False();

			FakePermissions.HasPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);

			result = Target.CreateTeamSchedulePermissionViewModel();
			result.ShiftTradePermisssion.Should().Be.True();
		}

		[Test]
		public void ShouldViewTeamsPermissionIsTrueWhenAvailableDataRangeIsNotMyOwnOrNone()
		{
			var result = Target.CreateTeamSchedulePermissionViewModel();
			result.ViewTeamsPermission.Should().Be.False();

			var siteRole = ApplicationRoleFactory.CreateRole("role", "own role");
			siteRole.AvailableData = new AvailableData();
			siteRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MySite;

			LoggedOnUser.CurrentUser().PermissionInformation.AddApplicationRole(siteRole);

			result = Target.CreateTeamSchedulePermissionViewModel();
			result.ViewTeamsPermission.Should().Be.True();
		}

		[Test]
		public void ShouldViewTeamsPermissionIsFalseWhenAvailableDataRangeIsMyOwn()
		{

			var ownRole = ApplicationRoleFactory.CreateRole("role", "own role");
			ownRole.AvailableData = new AvailableData();
			ownRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MyOwn;

			LoggedOnUser.CurrentUser().PermissionInformation.AddApplicationRole(ownRole);

			var result = Target.CreateTeamSchedulePermissionViewModel();
			result.ViewTeamsPermission.Should().Be.False();
		}

		[Test]
		public void ShouldViewTeamsPermissionIsFalseWhenAvailableDataRangeIsNone()
		{

			var noneRole = ApplicationRoleFactory.CreateRole("role", "own role");
			noneRole.AvailableData = new AvailableData();
			noneRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.None;

			LoggedOnUser.CurrentUser().PermissionInformation.AddApplicationRole(noneRole);

			var result = Target.CreateTeamSchedulePermissionViewModel();
			result.ViewTeamsPermission.Should().Be.False();
		}


	}
}