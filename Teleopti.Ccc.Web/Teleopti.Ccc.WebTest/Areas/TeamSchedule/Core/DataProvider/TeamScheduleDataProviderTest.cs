using NUnit.Framework;
using SharpTestsEx;
using System.Drawing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.DataProvider
{
	[TestFixture, TeamScheduleTest]
	public class TeamScheduleDataProviderTest
	{
		public IActivityProvider Target;
		public FakeActivityRepository ActivityRepository;
		[Test]
		public void ShouldRetrieveAllActivityTypes()
		{
			var activity = ActivityFactory.CreateActivity("ac1", Color.Green);
			ActivityRepository.Add(activity);

			var results = Target.GetAll();

			results.Count.Should().Be.EqualTo(1);
			results[0].Name.Should().Be.EqualTo("ac1");
			results[0].Color.Should().Be.EqualTo(Color.Green.ToHtml());
		}
	}
}
