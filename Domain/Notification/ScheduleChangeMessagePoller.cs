using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ScheduleChangeMessagePoller
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonScheduleChangeMessageRepository _scheduleChangeMsgRepo;

		public ScheduleChangeMessagePoller(
			ILoggedOnUser loggedOnUser, 
			IPersonScheduleChangeMessageRepository scheduleChangeMsgRepo)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleChangeMsgRepo = scheduleChangeMsgRepo;
		}

		public IDictionary<PollerInputPeriod, IList<ScheduleUpdatedPeriod>> Check(params PollerInputPeriod[] periods)
		{
			var userId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault();
			var messages = _scheduleChangeMsgRepo.PopMessages(userId).ToList();

			return periods.ToDictionary(period => period, period => checkForPeriod(messages, period));
		}

		private IList<ScheduleUpdatedPeriod> checkForPeriod(IList<PersonScheduleChangeMessage> messages, PollerInputPeriod period)
		{
			var userTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var periodInUtc = new DateTimePeriod(
				TimeZoneHelper.ConvertToUtc(period.StartDateTime, userTimezone),
				TimeZoneHelper.ConvertToUtc(period.EndDateTime, userTimezone));

			return messages
				.Where(n => periodInUtc.StartDateTime.Date <= n.EndDate.Date
						&& periodInUtc.EndDateTime.Date >= n.StartDate.Date)
				.Select(n => new ScheduleUpdatedPeriod
				{
					StartDate = TimeZoneHelper.ConvertFromUtc(n.StartDate, userTimezone).ToShortDateString(),
					EndDate = TimeZoneHelper.ConvertFromUtc(n.EndDate, userTimezone).ToShortDateString(),
				})
				.ToList();
		}

		public void ResetPolling()
		{
			_scheduleChangeMsgRepo.PopMessages(_loggedOnUser.CurrentUser().Id.Value);
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
