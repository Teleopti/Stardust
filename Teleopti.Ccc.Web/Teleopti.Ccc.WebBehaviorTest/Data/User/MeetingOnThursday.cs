using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class MeetingOnThursday : IUserDataSetup
	{
		public string MeetingLocation = "ADB-rummet";
		public string MeetingSubject = "Utbildning";

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var meeting = new Meeting(user,
			                          new[] { new MeetingPerson(user, false) },
			                          MeetingSubject, MeetingLocation,
			                          "A nice description of the meeting.",
			                          TestData.ActivityTraining, TestData.Scenario)
			              	{
			              		StartDate = DateOnly.Today.AddDays(-7),
			              		EndDate = DateOnly.Today.AddDays(7),
			              		StartTime = TimeSpan.FromHours(11),
			              		EndTime = TimeSpan.FromHours(11.5)
			              	};

			var recurrent = new RecurrentWeeklyMeeting { IncrementCount = 1 };
			recurrent[DayOfWeek.Thursday] = true;

			meeting.SetRecurrentOption(recurrent);

			var repository = new MeetingRepository(uow);
			repository.Add(meeting);
		}
	}
}