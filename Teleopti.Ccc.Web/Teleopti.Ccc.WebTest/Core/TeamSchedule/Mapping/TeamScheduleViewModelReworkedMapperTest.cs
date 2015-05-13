using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.Mapping;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.Mapping
{
	[TestFixture]
	[TeamScheduleTestAttribute]
	public class TeamScheduleViewModelReworkedMapperTest
	{
		public ITeamScheduleViewModelReworkedMapper Mapper;
		[Test]
		public void ShouldMap()
		{
			var target = Mapper.Map(new TeamScheduleViewModelData());
			target.Should().Not.Be.Null();
		}
	}
}
