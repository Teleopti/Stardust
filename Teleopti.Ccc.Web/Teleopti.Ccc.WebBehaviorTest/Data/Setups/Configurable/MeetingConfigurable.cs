using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class MeetingConfigurable : IUserDataSetup
	{
		public string Subject { get; set; }
		public string Location { get; set; }
		public string Description { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var activity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Purple) };
			var activityRepository = new ActivityRepository(uow);
			activityRepository.Add(activity);

			var scenario = GlobalDataMaker.Data().Data<DefaultScenario>().Scenario;
			var meeting = new Meeting(user,
									  new[] { new MeetingPerson(user, false) },
									  Subject, Location, Description ?? String.Empty,
									  activity, scenario)
			{
				StartDate = new DateOnly(StartTime),
				EndDate = new DateOnly(EndTime),
				StartTime = StartTime.TimeOfDay,
				EndTime = EndTime.TimeOfDay
			};

			var repository = new MeetingRepository(uow);
			repository.Add(meeting);
		}
	}
}