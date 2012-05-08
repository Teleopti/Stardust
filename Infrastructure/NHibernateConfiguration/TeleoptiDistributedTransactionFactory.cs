using System;
using System.Security.Principal;
using System.Threading;
using System.Transactions;
using log4net;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Transaction;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class TeleoptiDistributedTransactionFactory : AdoNetWithDistributedTransactionFactory
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TeleoptiDistributedTransactionFactory));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class TeleoptiDistributedTransactionContext : ITransactionContext, IEnlistmentNotification
		{
			private readonly ISessionImplementor _sessionImplementor;
			private readonly IPrincipal _currentPrincipal;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Transation")]
			public Transaction AmbientTransation { get; set; }
			public bool ShouldCloseSessionOnDistributedTransactionCompleted { get; set; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InActive")]
			public bool IsInActiveTransaction { get; set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Implementor"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
			public TeleoptiDistributedTransactionContext(ISessionImplementor sessionImplementor, Transaction transaction)
			{
				_currentPrincipal = Thread.CurrentPrincipal;
				_sessionImplementor = sessionImplementor;
				AmbientTransation = transaction.Clone();
				IsInActiveTransaction = true;
			}

			void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
			{
				using (new SessionIdLoggingContext(_sessionImplementor.SessionId))
				{
					Thread.CurrentPrincipal = _currentPrincipal;
					try
					{
						using (var transactionScope = new TransactionScope(AmbientTransation))
						{
							_sessionImplementor.BeforeTransactionCompletion(null);
							if (_sessionImplementor.FlushMode != FlushMode.Never && _sessionImplementor.ConnectionManager.IsConnected)
							{
								using (_sessionImplementor.ConnectionManager.FlushingFromDtcTransaction)
								{
									Logger.Debug(string.Format("[session-id={0}] Flushing from Dtc Transaction", _sessionImplementor.SessionId));
									_sessionImplementor.Flush();
								}
							}
							Logger.Debug("prepared for DTC transaction");
							transactionScope.Complete();
						}
						preparingEnlistment.Prepared();
					}
					catch (Exception ex)
					{
						Logger.Error("DTC transaction prepre phase failed", ex);
						preparingEnlistment.ForceRollback(ex);
					}
				}
			}
			void IEnlistmentNotification.Commit(Enlistment enlistment)
			{
				using (new SessionIdLoggingContext(_sessionImplementor.SessionId))
				{
					Thread.CurrentPrincipal = _currentPrincipal;
					Logger.Debug("committing DTC transaction");
					enlistment.Done();
					IsInActiveTransaction = false;
				}
			}
			void IEnlistmentNotification.Rollback(Enlistment enlistment)
			{
				using (new SessionIdLoggingContext(_sessionImplementor.SessionId))
				{
					Thread.CurrentPrincipal = _currentPrincipal;
					_sessionImplementor.AfterTransactionCompletion(false, null);
					Logger.Debug("rolled back DTC transaction");
					enlistment.Done();
					IsInActiveTransaction = false;
				}
			}
			void IEnlistmentNotification.InDoubt(Enlistment enlistment)
			{
				using (new SessionIdLoggingContext(_sessionImplementor.SessionId))
				{
					Thread.CurrentPrincipal = _currentPrincipal;
					_sessionImplementor.AfterTransactionCompletion(false, null);
					Logger.Debug("DTC transaction is in doubt");
					enlistment.Done();
					IsInActiveTransaction = false;
				}
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
			public void Dispose()
			{
				if (AmbientTransation != null)
				{
					AmbientTransation.Dispose();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "log4net.ILog.DebugFormat(System.String,System.Object[])"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public new void EnlistInDistributedTransactionIfNeeded(ISessionImplementor session)
		{
			if (session.TransactionContext != null)
			{
				return;
			}
			if (Transaction.Current == null)
			{
				return;
			}
			var transactionContext = new TeleoptiDistributedTransactionContext(session, Transaction.Current);
			session.TransactionContext = transactionContext;
			Logger.DebugFormat("enlisted into DTC transaction: {0}", new object[]{transactionContext.AmbientTransation.IsolationLevel });
			session.AfterTransactionBegin(null);
			transactionContext.AmbientTransation.TransactionCompleted += delegate(object sender, TransactionEventArgs e)
			                                                             	{
			                                                             		using (new SessionIdLoggingContext(session.SessionId))
			                                                             		{
			                                                             			((TeleoptiDistributedTransactionContext)session.TransactionContext).IsInActiveTransaction = false;
			                                                             			bool successful = false;
			                                                             			try
			                                                             			{
			                                                             				successful =
			                                                             					(e.Transaction.TransactionInformation.Status ==
			                                                             					 TransactionStatus.Committed);
			                                                             			}
			                                                             			catch (ObjectDisposedException exception)
			                                                             			{
			                                                             				Logger.Warn(
			                                                             					"Completed transaction was disposed, assuming transaction rollback",
			                                                             					exception);
			                                                             			}
			                                                             			session.AfterTransactionCompletion(successful, null);
			                                                             			if (transactionContext.ShouldCloseSessionOnDistributedTransactionCompleted)
			                                                             			{
			                                                             				session.CloseSessionFromDistributedTransaction();
			                                                             			}
			                                                             			session.TransactionContext = null;
			                                                             		}
			                                                             	};
			transactionContext.AmbientTransation.EnlistVolatile(transactionContext, EnlistmentOptions.EnlistDuringPrepareRequired);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public new bool IsInDistributedActiveTransaction(ISessionImplementor session)
		{
			var distributedTransactionContext = (TeleoptiDistributedTransactionContext)session.TransactionContext;
			return distributedTransactionContext != null && distributedTransactionContext.IsInActiveTransaction;
		}
	}
}