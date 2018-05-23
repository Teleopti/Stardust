using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	[TestFixture]
	public class TeamSchedulePermissionViewModelFactoryTest
	{
		[Test]
		public void PermissionForShiftTradeBulletinBoard_WhenAgentHasPermissionToViewShiftTradeBulletinBoardWithDefaultContructor_ShouldBeTrue()
		{
			var target = new TeamSchedulePermissionViewModelFactory(new FakePermissionProvider());

			var result = target.CreateTeamSchedulePermissionViewModel();
			Assert.That(result.ShiftTradeBulletinBoardPermission, Is.True);
		}
	}
}
