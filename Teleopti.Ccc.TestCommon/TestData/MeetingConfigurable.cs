using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public class MeetingConfigurable : IUserDataSetup
	{
		public string Subject { get; set; }
		public string Location { get; set; }
		public string Description { get; set; }
		public IScenario Scenario { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }


		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var activity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Purple) };
			var activityRepository = new ActivityRepository(currentUnitOfWork);
			activityRepository.Add(activity);
			
			if (Scenario == null)
				Scenario = DefaultScenario.Scenario;
			

			var meeting = new Meeting(user,
									  new[] { new MeetingPerson(user, false) },
									  Subject, Location, Description ?? String.Empty,
									  activity, Scenario)
			{
				StartDate = new DateOnly(StartTime),
				EndDate = new DateOnly(EndTime),
				StartTime = StartTime.TimeOfDay,
				EndTime = EndTime.TimeOfDay
			};
			var repository = new MeetingRepository(currentUnitOfWork);
			repository.Add(meeting);
		}
	}
}