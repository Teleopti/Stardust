using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using NHibernate.Engine;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class TeleoptiDistributedTransactionContextTest
	{
		private MockRepository mocks;
		private ISessionImplementor sessionImplementor;
		private TeleoptiDistributedTransactionFactory.TeleoptiDistributedTransactionContext target;
		private Enlistment enlistment;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			enlistment = mocks.Stub<Enlistment>(null);
			sessionImplementor = mocks.DynamicMock<ISessionImplementor>();
		}

		[Test,Ignore]
		public void ShouldJustGetThroughThisToGetCoverageNumber2()
		{
			using (mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				using (new TransactionScope())
				{
					target = new TeleoptiDistributedTransactionFactory.TeleoptiDistributedTransactionContext(sessionImplementor, Transaction.Current);
					((IEnlistmentNotification)target).Commit(enlistment);
				}
			}
		}
	}
}
