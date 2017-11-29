using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ScheduleChangeMessagePoller
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IASMScheduleChangeTimeRepository _scheduleChangeTimeRepo;
		private readonly INow _now;
		private TimeSpan interval = TimeSpan.FromMinutes(5);

		public ScheduleChangeMessagePoller(
			ILoggedOnUser loggedOnUser,
			IASMScheduleChangeTimeRepository scheduleChangeTimeRepo,
			INow now)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleChangeTimeRepo = scheduleChangeTimeRepo;
			_now = now;
		}

		public bool Check()
		{
			var userId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault();
			var time = _scheduleChangeTimeRepo.GetScheduleChangeTime(userId);
			if (time == null) return false;
			return _now.UtcDateTime() - time.TimeStamp <= interval;
		}
	}

	public class PollerInputPeriod
	{
		public PollerInputPeriod() { }
		public PollerInputPeriod(DateTime startDateTime, DateTime endDateTime)
		{
			StartDateTime = startDateTime;
			EndDateTime = endDateTime;
		}
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}

	public class ScheduleUpdatedPeriod
	{
		public string StartDate { get; set; }
		public string EndDate { get; set; }
	}
}
