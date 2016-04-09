using NHibernate;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ApplicationUnitOfWork : NHibernateUnitOfWork
	{
		public ApplicationUnitOfWork(UnitOfWorkContext context, ISession session, TransactionIsolationLevel isolationLevel, ICurrentTransactionHooks transactionHooks) : base(context, session, isolationLevel, transactionHooks)
		{
		}

		protected override void BlackSheep()
		{
			// this is a very bad idea
			// the reason there is 2 flushes is because the first one catches any child, and the second one updates the roots version number
			// changes here will just be included in the second flush, and therefor that behavior will not work for aggregates modified here!
			// ---> this behavior belongs in the domain!
			new SendPushMessageWhenRootAlteredService()
				.SendPushMessages(
					Interceptor.Value.ModifiedRoots,
					new PushMessagePersister(
						new PushMessageRepository(this),
						new PushMessageDialogueRepository(this),
						new CreatePushMessageDialoguesService()
						));
		}

	}
}