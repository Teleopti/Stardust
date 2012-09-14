using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class MeetingConfigurable : IUserDataSetup
	{
		public string Subject { get; set; }
		public string Location { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
			var meeting = new Meeting(user,
									  new[] { new MeetingPerson(user, false) },
									  Subject, Location,
									  "A nice description of the meeting.",
									  TestData.ActivityTraining, scenario)
			{
				StartDate = new DateOnly(StartTime),
				EndDate = new DateOnly(EndTime),
				StartTime = StartTime.TimeOfDay,
				EndTime = EndTime.TimeOfDay
			};

			//var recurrent = new RecurrentWeeklyMeeting { IncrementCount = 1 };
			//recurrent[DayOfWeek.Thursday] = true;
			//meeting.SetRecurrentOption(recurrent);

			var repository = new MeetingRepository(uow);
			repository.Add(meeting);
		}
	}
}