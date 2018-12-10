using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Global;


namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class NotificationsControllerTest
	{
		[Test]
		public void ShouldReturnPersistedNotifications()
		{
			var fakeJobResultRepository = new FakeJobResultRepository();
			var person = new Person().WithName(new Name("aa", "bb"));
			var timeStamp = new DateTime(2015, 03, 18, 9, 18, 0, DateTimeKind.Utc);
			
			var jobResult1 = getJobResult(false, person, timeStamp);
			var jobResult2 = getJobResult(true, person, timeStamp);

			fakeJobResultRepository.Add(jobResult1);
			fakeJobResultRepository.Add(jobResult2);
			IUserTimeZone userTimeZone = new HawaiiTimeZone();
			var target = new NotificationsController(fakeJobResultRepository,userTimeZone);
			var result = (OkNegotiatedContentResult<JobResultNotificationModel[]>)target.GetNotifications();
			result.Content.Length.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnPersistedNotificationsContent()
		{
			var fakeJobResultRepository = new FakeJobResultRepository();
			var person = new Person().WithName(new Name("aa", "bb"));
			var timeStamp = DateTime.UtcNow.AddHours(1);
			IUserTimeZone userTimeZone = new HawaiiTimeZone();
			
			var jobResult1 = getJobResult(false, person, timeStamp);
			var jobResult2 = getJobResult(true, person, timeStamp);

			fakeJobResultRepository.Add(jobResult1);
			fakeJobResultRepository.Add(jobResult2);

			
			var target = new NotificationsController(fakeJobResultRepository, userTimeZone);

			var result = (OkNegotiatedContentResult<JobResultNotificationModel[]>)target.GetNotifications();
			var  firstItem = result.Content.First();
			firstItem.JobCategory.Should().Be.EqualTo(JobCategory.QuickForecast);
			firstItem.Owner.Should().Be.EqualTo(person.Name.ToString());
			firstItem.Timestamp.Should().Be.EqualTo(TimeZoneInfo.ConvertTimeFromUtc(timeStamp,userTimeZone.TimeZone()));
			firstItem.Status.Should().Be.EqualTo(UserTexts.Resources.WorkingThreeDots);
		}
		
		[Test]
		public void ShouldReturnLatestFiveNotifications()
		{
			var fakeJobResultRepository = new FakeJobResultRepository();
			var person = new Person().WithName(new Name("aa", "bb"));
			IUserTimeZone userTimeZone = new HawaiiTimeZone();
			var timeStamp = new DateTime(2015, 03, 18, 9, 18, 0, DateTimeKind.Utc);
			
			var jobResult1 = getJobResult(false, person, timeStamp.AddMinutes(1));
			var jobResult2 = getJobResult(true, person, timeStamp.AddMinutes(2));
			var jobResult3 = getJobResult(true, person, timeStamp.AddMinutes(3));
			var jobResult4 = getJobResult(true, person, timeStamp.AddMinutes(4));
			var jobResult5 = getJobResult(true, person, timeStamp.AddMinutes(5));
			var jobResult6 = getJobResult(true, person, timeStamp.AddMinutes(6));
			
			fakeJobResultRepository.Add(jobResult1);
			fakeJobResultRepository.Add(jobResult2);
			fakeJobResultRepository.Add(jobResult3);
			fakeJobResultRepository.Add(jobResult4);
			fakeJobResultRepository.Add(jobResult5);
			fakeJobResultRepository.Add(jobResult6);
			
			var target = new NotificationsController(fakeJobResultRepository, userTimeZone);

			var result = (OkNegotiatedContentResult<JobResultNotificationModel[]>)target.GetNotifications();
			result.Content.First().Timestamp.Should()
				.Be.EqualTo(TimeZoneInfo.ConvertTimeFromUtc(timeStamp.AddMinutes(5), userTimeZone.TimeZone()));
			result.Content.Last().Timestamp.Should()
				.Be.EqualTo(TimeZoneInfo.ConvertTimeFromUtc(timeStamp.AddMinutes(1), userTimeZone.TimeZone()));

		}

		private JobResult getJobResult(bool finishedOk, IPerson person, DateTime timeStamp)
		{
			return  new JobResult(JobCategory.QuickForecast, new DateOnlyPeriod(2015, 03, 16, 2015, 03, 17), person,
				timeStamp) { FinishedOk = finishedOk };
		}
	}
}
