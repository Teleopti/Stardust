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
		private readonly IMailboxRepository _mailboxRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ICurrentScenario _currentScenario;
		private readonly INow _now;
		private readonly TimeSpan _expirationInterval = TimeSpan.FromMinutes(15);

		public ScheduleChangeMailboxPoller(
			IMailboxRepository mailboxRepository,
			ILoggedOnUser loggedOnUser,
			ICurrentDataSource currentDataSource,
			ICurrentBusinessUnit currentBusinessUnit,
			ICurrentScenario currentScenario,
			INow now)
		{
			_mailboxRepository = mailboxRepository;
			_loggedOnUser = loggedOnUser;
			_currentDataSource = currentDataSource;
			_currentBusinessUnit = currentBusinessUnit;
			_currentScenario = currentScenario;
			_now = now;
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

		public bool Check(Guid mailboxId, DateOnlyPeriod period)
		{
			var notifications = PopMessages(mailboxId);
			var userId = _loggedOnUser.CurrentUser().Id.ToString();
			return notifications.Any(n => n.BinaryData.Contains(userId)
					&& period.StartDate <= new DateOnly(n.EndDateAsDateTime())
					&& period.EndDate >= new DateOnly(n.StartDateAsDateTime()));
		}


		public IEnumerable<Message> PopMessages(Guid mailboxId)
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
				"/",
				new[]
				{
					_currentDataSource.CurrentName(),
					_currentBusinessUnit.Current().Id.ToString(),
					nameof(IScheduleChangedMessage),
					"ref",
					_currentScenario.Current().Id.ToString()
				});
		}
	}
}
