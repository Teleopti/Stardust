using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
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
			var user = MockRepository.GenerateMock<ILoggedOnUser>();
			var groupScheduleProvider = MockRepository.GenerateMock<IGroupScheduleViewModelFactory>();
			var groupId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var schedules = new[] {new GroupScheduleShiftViewModel {PersonId = Guid.NewGuid().ToString()}};
			groupScheduleProvider.Stub(x => x.CreateViewModel(groupId, dateTime)).Return(schedules);

			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			userTimeZone.Stub(x => x.TimeZone()).Return(TimeZoneInfo.Utc);

			user.Stub(x => x.CurrentUser().PermissionInformation.DefaultTimeZone()).Return(userTimeZone.TimeZone());

			var target = new GroupScheduleHub(groupScheduleProvider, user);
			var hubBuilder = new TestHubBuilder();

			dynamic actual = null;
			var hubClient = hubBuilder.FakeClient<dynamic>("incomingGroupSchedule", a => { actual = a; });
			hubBuilder.SetupHub(target, hubClient);

			target.SubscribeGroupSchedule(groupId, dateTime);

			var actualBaseDate = actual.BaseDate;
			var actualTotalCount = actual.TotalCount;
			var actualSchedules = new List<GroupScheduleShiftViewModel>(actual.Schedules);
			Assert.That(actualBaseDate, Is.EqualTo(dateTime));
			Assert.That(actualSchedules.Single().PersonId, Is.EqualTo(schedules.First().PersonId));
			Assert.That(actualTotalCount, Is.EqualTo(1));
		}

		[Test]
		public void ShouldPushDataToCallerOnSubscribeWhenNoScheduleYet()
		{
			var user = MockRepository.GenerateMock<ILoggedOnUser>();
			var groupScheduleProvider = MockRepository.GenerateMock<IGroupScheduleViewModelFactory>();
			var groupId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			groupScheduleProvider.Stub(x => x.CreateViewModel(groupId, dateTime)).Return(new GroupScheduleShiftViewModel[] { });

			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			userTimeZone.Stub(x => x.TimeZone()).Return(TimeZoneInfo.Utc);

			user.Stub(x => x.CurrentUser().PermissionInformation.DefaultTimeZone()).Return(userTimeZone.TimeZone());

			var target = new GroupScheduleHub(groupScheduleProvider, user);
			var hubBuilder = new TestHubBuilder();

			dynamic actual = null;
			var hubClient = hubBuilder.FakeClient<dynamic>("incomingGroupSchedule", a => { actual = a; });
			hubBuilder.SetupHub(target, hubClient);

			target.SubscribeGroupSchedule(groupId, dateTime);

			var actualBaseDate = actual.BaseDate;
			var actualTotalCount = actual.TotalCount;
			var actualSchedules = new List<GroupScheduleShiftViewModel>(actual.Schedules);
			Assert.That(actualBaseDate, Is.EqualTo(dateTime));
			Assert.That(actualSchedules.Count, Is.EqualTo(0));
			Assert.That(actualTotalCount, Is.EqualTo(0));
		}
	}
}