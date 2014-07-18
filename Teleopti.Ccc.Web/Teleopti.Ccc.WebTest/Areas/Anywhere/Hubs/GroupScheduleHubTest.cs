using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class GroupScheduleHubTest
	{
		[Test]
		public void ShouldPushDataToCallerOnSubscribe()
		{
			var groupScheduleProvider = MockRepository.GenerateMock<IGroupScheduleViewModelFactory>();
			var groupId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var data = new[] { new GroupScheduleShiftViewModel { PersonId = Guid.NewGuid().ToString() } };
			groupScheduleProvider.Stub(x => x.CreateViewModel(groupId, dateTime)).Return(data);
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			userTimeZone.Stub(x => x.TimeZone()).Return(TimeZoneInfo.Utc);
			var target = new GroupScheduleHub(groupScheduleProvider);
			var hubBuilder = new TestHubBuilder();
			IEnumerable<dynamic> actual = null;
			hubBuilder.SetupHub(target, hubBuilder.FakeClient<IEnumerable<dynamic>>("incomingGroupSchedule", a => { actual = a; }));

			target.SubscribeGroupSchedule(groupId, dateTime);

			Assert.That(actual.Single().PersonId, Is.EqualTo(data.First().PersonId));
		}

	}
}