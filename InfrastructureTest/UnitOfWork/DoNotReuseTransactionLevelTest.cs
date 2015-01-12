using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	public class DoNotReuseTransactionLevelTest : DatabaseTest
	{
		[Test]
		public void ShouldBeResetAfterExplcitlySetInEarlierTransaction()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork(TransactionIsolationLevel.Serializable))
			{
				startTransactionByMakingDummyCall(uow);
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				serverTransactionLevel(uow)
					.Should().Be.EqualTo(2);
			}
		}

		private static void startTransactionByMakingDummyCall(IUnitOfWork uow)
		{
			uow.FetchSession().CreateSQLQuery("select 1").UniqueResult<int>();
		}

		/// <returns>
		/// 0 = Unspecified
		/// 1 = ReadUncomitted
		/// 2 = ReadCommitted
		/// 3 = Repeatable
		/// 4 = Serializable
		/// 5 = Snapshot
		/// </returns>
		private static int serverTransactionLevel(IUnitOfWork uow)
		{
			const string sql = "select transaction_isolation_level from sys.dm_exec_sessions where session_id=@@spid";
			return uow.FetchSession().CreateSQLQuery(sql).UniqueResult<short>();
		}
	}
}