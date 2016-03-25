using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
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
			var activity = ActivityFactory.CreateActivity("ac1");
			ActivityRepository.Add(activity);

			var results = Target.GetAll();

			results.Count.Should().Be.EqualTo(1);
		}
	}
}
