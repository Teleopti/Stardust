using System;
using System.Transactions;
using NHibernate.Engine;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class TeleoptiDistributedTransactionFactoryTest
	{
		private TeleoptiDistributedTransactionFactory target;
		private MockRepository mocks;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			target = new TeleoptiDistributedTransactionFactory();
		}

		[Test]
		public void ShouldNotBeInDistributedTransaction()
		{
			var sessionImplementor = mocks.DynamicMock<ISessionImplementor>();
			using(mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				target.IsInDistributedActiveTransaction(sessionImplementor).Should().Be.False();
			}
		}

		[Test]
		public void ShouldJustGetThroughThisToGetCoverage1()
		{
			var sessionImplementor = mocks.DynamicMock<ISessionImplementor>();
			using (mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				target.EnlistInDistributedTransactionIfNeeded(sessionImplementor);
			}
		}

		[Test]
		public void ShouldJustGetThroughThisToGetCoverage2()
		{
			var sessionImplementor = mocks.StrictMock<ISessionImplementor>();
			var sessionId = Guid.NewGuid();
			using (var transaction = new TransactionScope())
			{
				var context = new TeleoptiDistributedTransactionFactory.TeleoptiDistributedTransactionContext(sessionImplementor,
				                                                                                              Transaction.Current);
				using (mocks.Record())
				{
					Expect.Call(sessionImplementor.TransactionContext = context).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(sessionImplementor.TransactionContext).Return(null);
					Expect.Call(sessionImplementor.TransactionContext).Return(context);
					Expect.Call(() => sessionImplementor.AfterTransactionBegin(null));
					Expect.Call(() => sessionImplementor.AfterTransactionCompletion(false,null)).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(sessionImplementor.SessionId).Return(sessionId).Repeat.AtLeastOnce();
				}
				using (mocks.Playback())
				{
					target.EnlistInDistributedTransactionIfNeeded(sessionImplementor);
					transaction.Dispose();
				}
			}
		}

		[Test]
		public void ShouldJustGetThroughThisToGetCoverage3()
		{
			var sessionImplementor = mocks.StrictMock<ISessionImplementor>();
			var sessionId = Guid.NewGuid();
			using (var transaction = new TransactionScope())
			{
				var context = new TeleoptiDistributedTransactionFactory.TeleoptiDistributedTransactionContext(sessionImplementor,
																											  Transaction.Current);
				using (mocks.Record())
				{
					Expect.Call(sessionImplementor.TransactionContext = context).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(sessionImplementor.TransactionContext).Return(null);
					Expect.Call(sessionImplementor.TransactionContext).Return(context);
					Expect.Call(sessionImplementor.FlushMode).Return(NHibernate.FlushMode.Never);
					Expect.Call(() => sessionImplementor.AfterTransactionBegin(null));
					Expect.Call(() => sessionImplementor.BeforeTransactionCompletion(null));
					Expect.Call(() => sessionImplementor.AfterTransactionCompletion(false, null)).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(sessionImplementor.SessionId).Return(sessionId).Repeat.AtLeastOnce();
				}
				using (mocks.Playback())
				{
					target.EnlistInDistributedTransactionIfNeeded(sessionImplementor);
					transaction.Complete();
					transaction.Dispose();
				}
			}
		}

		[Test]
		public void ShouldJustGetThroughThisToGetCoverage4()
		{
			var sessionImplementor = mocks.StrictMock<ISessionImplementor>();
			var sessionId = Guid.NewGuid();
			using (var transaction = new TransactionScope())
			{
				var context = new TeleoptiDistributedTransactionFactory.TeleoptiDistributedTransactionContext(sessionImplementor,
																											  Transaction.Current);
				using (mocks.Record())
				{
					Expect.Call(sessionImplementor.TransactionContext = context).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(sessionImplementor.TransactionContext).Return(null);
					Expect.Call(sessionImplementor.TransactionContext).Return(context);
					Expect.Call(() => sessionImplementor.AfterTransactionBegin(null));
					Expect.Call(() => sessionImplementor.BeforeTransactionCompletion(null)).Throw(new Exception("Error!"));
					Expect.Call(() => sessionImplementor.AfterTransactionCompletion(false, null)).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(sessionImplementor.SessionId).Return(sessionId).Repeat.AtLeastOnce();
				}
				using (mocks.Playback())
				{
					target.EnlistInDistributedTransactionIfNeeded(sessionImplementor);
					transaction.Complete();
					Assert.Throws<TransactionAbortedException>(transaction.Dispose);
				}
			}
		}
	}
}