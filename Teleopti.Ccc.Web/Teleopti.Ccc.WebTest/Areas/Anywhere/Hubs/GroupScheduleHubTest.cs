using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class GroupScheduleHubTest
	{
		[Test]
		public void ShouldPushDataToCallerOnSubscribe()
		{
			var teamScheduleProvider = MockRepository.GenerateMock<IGroupScheduleViewModelFactory>();
			var teamId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var data = new[] { new GroupScheduleShiftViewModel { PersonId = Guid.NewGuid().ToString() } };
			teamScheduleProvider.Stub(x => x.CreateViewModel(teamId, dateTime)).Return(data);
			var target = new GroupScheduleHub(teamScheduleProvider);
			var hubBuilder = new TestHubBuilder();
			IEnumerable<dynamic> actual = null;
			hubBuilder.SetupHub(target, hubBuilder.FakeClient<IEnumerable<dynamic>>("incomingGroupSchedule", a => { actual = a; }));

			target.SubscribeGroupSchedule(teamId, dateTime);

			Assert.That(actual.Single().PersonId, Is.EqualTo(data.First().PersonId));
		}

	}
}