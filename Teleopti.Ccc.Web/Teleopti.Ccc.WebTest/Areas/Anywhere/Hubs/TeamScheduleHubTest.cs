using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class TeamScheduleHubTest
	{
		[Test]
		public void ShouldPushDataToCallerOnSubscribe()
		{
			var teamScheduleProvider = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var teamId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var data = new[] { new TeamScheduleShiftViewModel { FirstName = "Pierre" } };
			teamScheduleProvider.Stub(x => x.CreateViewModel(teamId, dateTime)).Return(data);
			var target = new TeamScheduleHub(teamScheduleProvider);
			var hubBuilder = new TestHubBuilder();
			IEnumerable<dynamic> actual = null;
			hubBuilder.SetupHub(target, hubBuilder.FakeClient<IEnumerable<dynamic>>("incomingTeamSchedule", a => { actual = a; }));

			target.SubscribeTeamSchedule(teamId, dateTime);

			Assert.That(actual.Single().FirstName, Is.EqualTo("Pierre"));
		}

	}
}