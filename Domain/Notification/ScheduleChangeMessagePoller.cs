using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ScheduleChangeMessagePoller
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public ScheduleChangeMessagePoller(
			ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public IDictionary<PollerInputPeriod, IList<ScheduleUpdatedPeriod>> Check(Guid mailboxId, params PollerInputPeriod[] periods)
		{
			//var userId = _loggedOnUser.CurrentUser().Id.ToString();
			//var notifications = _messageBrokerServer
			//	.PopMessages(mailboxId)
			//	.Where(n => n.BinaryData.Contains(userId))
			//	.ToList();

			//return periods.ToDictionary(period => period, period => checkForPeriod(notifications, period));
			return null;
		}

		private IList<ScheduleUpdatedPeriod> checkForPeriod(IList<Message> notifications, PollerInputPeriod period)
		{
			var userTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var periodInUtc = new DateTimePeriod(
				TimeZoneHelper.ConvertToUtc(period.StartDateTime, userTimezone),
				TimeZoneHelper.ConvertToUtc(period.EndDateTime, userTimezone));

			return notifications
				.Where(n => periodInUtc.StartDateTime.Date <= n.EndDateAsDateTime().Date
						&& periodInUtc.EndDateTime.Date >= n.StartDateAsDateTime().Date)
				.Select(n => new ScheduleUpdatedPeriod
				{
					StartDate = TimeZoneHelper.ConvertFromUtc(n.StartDateAsDateTime(), userTimezone).ToShortDateString(),
					EndDate = TimeZoneHelper.ConvertFromUtc(n.EndDateAsDateTime(), userTimezone).ToShortDateString(),
				})
				.ToList();
		}

		public void ResetPolling(Guid mailboxId)
		{
			//TODO: remove messages belonging to logon user.
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
