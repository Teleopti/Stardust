using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	[EnabledBy(Toggles.MessageBroker_ActuallyPurgeEvery5Minutes_79140)]
	[RunInterval(5)]
	public class MessageBrokerMailboxPurger :
		IHandleEvent<SharedMinuteTickEvent>,
		IRunOnHangfire
	{
		private readonly IMailboxRepository _mailbox;

		public MessageBrokerMailboxPurger(IMailboxRepository mailbox)
		{
			_mailbox = mailbox;
		}

		[MessageBrokerUnitOfWork]
		public virtual void Handle(SharedMinuteTickEvent @event)
		{
			_mailbox.Purge();
		}
	}
}