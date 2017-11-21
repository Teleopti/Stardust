using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ScheduleChangeMailboxPoller
	{
		private readonly IMailboxRepository _mailboxRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ICurrentScenario _currentScenario;
		private readonly INow _now;
		private readonly TimeSpan _expirationInterval;

		public ScheduleChangeMailboxPoller(
			IMailboxRepository mailboxRepository,
			ILoggedOnUser loggedOnUser,
			ICurrentDataSource currentDataSource,
			ICurrentBusinessUnit currentBusinessUnit,
			ICurrentScenario currentScenario,
			INow now, 
			IConfigReader configReader)
		{
			_mailboxRepository = mailboxRepository;
			_loggedOnUser = loggedOnUser;
			_currentDataSource = currentDataSource;
			_currentBusinessUnit = currentBusinessUnit;
			_currentScenario = currentScenario;
			_now = now;
			_expirationInterval = TimeSpan.FromSeconds(configReader.ReadValue("MessageBrokerMailboxExpirationInSeconds", 60 * 15));
		}

		public Guid StartPolling()
		{
			var mailboxId = Guid.NewGuid();
			_mailboxRepository.Add(new Mailbox
			{
				Route = makeRoute(),
				Id = mailboxId,
				ExpiresAt = _now.UtcDateTime().Add(_expirationInterval)
			});
			return mailboxId;
		}

		public IList<DateOnlyPeriod> Check(Guid mailboxId, DateTime startDateInUserTimezone, DateTime endDateInUserTimezone)
		{
			var userTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var period = new DateOnlyPeriod(
				new DateOnly(TimeZoneHelper.ConvertToUtc(startDateInUserTimezone, userTimezone)),
				new DateOnly(TimeZoneHelper.ConvertToUtc(endDateInUserTimezone, userTimezone)));
			var notifications = popMessages(mailboxId);
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
			popMessages(mailboxId);
		}

		private IEnumerable<Message> popMessages(Guid mailboxId)
		{
			var mailbox = _mailboxRepository.Load(mailboxId);
			var updateExpirationAt = mailbox.ExpiresAt.Subtract(new TimeSpan(_expirationInterval.Ticks / 2));

			return _now.UtcDateTime() >= updateExpirationAt ?
				_mailboxRepository.PopMessages(mailboxId, _now.UtcDateTime().Add(_expirationInterval)) :
				_mailboxRepository.PopMessages(mailboxId, null);
		}

		private string makeRoute()
		{
			return string.Join(
				"/", _currentDataSource.CurrentName(), _currentBusinessUnit.Current().Id.ToString(),
				nameof(IScheduleChangedMessage), "ref", _currentScenario.Current().Id.ToString());
		}

		
	}
}
