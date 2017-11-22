using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public IList<DateOnlyPeriod> Check(Guid mailboxId, DateTime startDateInUserTimezone, DateTime endDateInUserTimezone)
		{
			var userTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var period = new DateOnlyPeriod(
				new DateOnly(TimeZoneHelper.ConvertToUtc(startDateInUserTimezone, userTimezone)),
				new DateOnly(TimeZoneHelper.ConvertToUtc(endDateInUserTimezone, userTimezone)));
			var notifications = _messageBrokerServer.PopMessages(mailboxId);
			var userId = _loggedOnUser.CurrentUser().Id.ToString();
			return notifications
				.Where(n => n.BinaryData.Contains(userId)
						&& period.StartDate <= new DateOnly(n.EndDateAsDateTime())
						&& period.EndDate >= new DateOnly(n.StartDateAsDateTime()))
				.Select(n => new DateOnlyPeriod(
						new DateOnly(TimeZoneHelper.ConvertFromUtc(startDateInUserTimezone, userTimezone)),
						new DateOnly(TimeZoneHelper.ConvertFromUtc(endDateInUserTimezone, userTimezone))))
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
}
