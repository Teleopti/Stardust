using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	[EnabledBy(Toggles.MessageBroker_VeganBurger_79140)]
	public class MessageBrokerMailboxPurger :
		IHandleEvent<SharedMinuteTickEvent>,
		IRunOnHangfire
	{
		private readonly IMailboxRepository _mailbox;

		public MessageBrokerMailboxPurger(IMailboxRepository mailbox)
		{
			_mailbox = mailbox;
		}

		public void Handle(SharedMinuteTickEvent @event)
		{
			PurgeSomeMailboxes();
			PurgeSomeNotifications();
		}

		[MessageBrokerUnitOfWork]
		protected virtual void PurgeSomeMailboxes() =>
			_mailbox.PurgeOneChunkOfMailboxes();
		
		[MessageBrokerUnitOfWork]
		protected virtual void PurgeSomeNotifications() =>
			_mailbox.PurgeOneChunkOfNotifications();
	}
}