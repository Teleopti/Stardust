using NUnit.Framework;
using SharpTestsEx;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

			var lunchActivity = ActivityFactory.CreateActivity("lunch", Color.Yellow);
			lunchActivity.ReportLevelDetail = ReportLevelDetail.Lunch;

			var shortBreakActivity = ActivityFactory.CreateActivity("short break", Color.Red);
			shortBreakActivity.ReportLevelDetail = ReportLevelDetail.ShortBreak;

			ActivityRepository.Add(activity);
			ActivityRepository.Add(lunchActivity);
			ActivityRepository.Add(shortBreakActivity);

			var results = Target.GetAll();

			results.Count.Should().Be.EqualTo(3);
			results[0].Name.Should().Be.EqualTo("ac1");
			results[0].Color.Should().Be.EqualTo(Color.Green.ToHtml());

			results[1].Name.Should().Be.EqualTo("lunch");
			results[1].Color.Should().Be.EqualTo(Color.Yellow.ToHtml());
			results[1].FloatOnTop.Should().Be.True();

			results[2].Name.Should().Be.EqualTo("short break");
			results[2].Color.Should().Be.EqualTo(Color.Red.ToHtml());
			results[2].FloatOnTop.Should().Be.True();
		}
	}
}
