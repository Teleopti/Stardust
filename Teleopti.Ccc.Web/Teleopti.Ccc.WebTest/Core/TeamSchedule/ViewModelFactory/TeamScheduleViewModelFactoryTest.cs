using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	[TestFixture]
	public class TeamScheduleViewModelFactoryTest
	{
		[Test]
		public void PermissionForShiftTradeBulletinBoard_WhenAgentHasPermissionToViewShiftTradeBulletinBoardWithDefaultContructor_ShouldBeTrue()
		{
			var target = new TeamScheduleViewModelFactory(new FakePermissionProvider());

			var result = target.CreateTeamSchedulePermissionViewModel();
			Assert.That(result.ShiftTradeBulletinBoardPermission, Is.True);
		}
	}
}
