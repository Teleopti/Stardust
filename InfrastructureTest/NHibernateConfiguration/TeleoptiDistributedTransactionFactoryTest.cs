using System;
using System.Data;
using System.Transactions;
using NHibernate;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Engine.Transaction;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using log4net.Util;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
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

		[Test]
		public void ShouldJustGetThroughThisToGetCoverage5()
		{
			using (mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				target.Configure(EmptyDictionary.Instance);
			}
		}

		[Test]
		public void ShouldJustGetThroughThisToGetCoverage6()
		{
			var sessionImplementor = mocks.DynamicMock<ISessionImplementor>();
			using (mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				target.CreateTransaction(sessionImplementor).Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldJustGetThroughThisToGetCoverage7()
		{
			var sessionImplementor = mocks.DynamicMock<ISessionImplementor>();
			var isolatedWork = mocks.DynamicMock<IIsolatedWork>();
			var sessionFactory = mocks.DynamicMock<ISessionFactoryImplementor>();
			var connectionProvider = mocks.DynamicMock<IConnectionProvider>();
			var connection = mocks.DynamicMock<IDbConnection>();
			using (mocks.Record())
			{
				Expect.Call(sessionImplementor.Factory).Return(sessionFactory);
				Expect.Call(sessionFactory.Dialect).Return(new MsSql2008Dialect()).Repeat.AtLeastOnce();
				Expect.Call(sessionFactory.ConnectionProvider).Return(connectionProvider).Repeat.AtLeastOnce();
				Expect.Call(connectionProvider.GetConnection()).Return(connection).Repeat.AtLeastOnce();
			}
			using (mocks.Playback())
			{
				target.ExecuteWorkInIsolation(sessionImplementor,isolatedWork,false);
			}
		}
	}
}