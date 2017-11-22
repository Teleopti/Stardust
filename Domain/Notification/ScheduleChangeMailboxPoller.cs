using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ScheduleChangeMailboxPoller
	{
		private readonly IMessageBrokerServer _messageBrokerServer;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ICurrentScenario _currentScenario;

		public ScheduleChangeMailboxPoller(
			ILoggedOnUser loggedOnUser,
			ICurrentDataSource currentDataSource,
			ICurrentBusinessUnit currentBusinessUnit,
			ICurrentScenario currentScenario,
			IMessageBrokerServer messageBrokerServer)
		{
			_loggedOnUser = loggedOnUser;
			_currentDataSource = currentDataSource;
			_currentBusinessUnit = currentBusinessUnit;
			_currentScenario = currentScenario;
			_messageBrokerServer = messageBrokerServer;
		}

		public Guid StartPolling()
		{
			var mailboxId = Guid.NewGuid();
			_messageBrokerServer.CreateMailbox(mailboxId, makeRoute());
			return mailboxId;
		}

		public IDictionary<PollerInputPeriod, IList<ScheduleUpdatedPeriod>> Check(Guid mailboxId, params PollerInputPeriod[] periods)
		{
			var userId = _loggedOnUser.CurrentUser().Id.ToString();
			var notifications = _messageBrokerServer
				.PopMessages(mailboxId)
				.Where(n => n.BinaryData.Contains(userId))
				.ToList();

			return periods.ToDictionary(period => period, period => checkForPeriod(notifications, period));
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
			_messageBrokerServer.PopMessages(mailboxId);
		}
		private string makeRoute()
		{
			return string.Join(
				"/", _currentDataSource.CurrentName(), _currentBusinessUnit.Current().Id.ToString(),
				nameof(IScheduleChangedMessage), "ref", _currentScenario.Current().Id.ToString());
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
