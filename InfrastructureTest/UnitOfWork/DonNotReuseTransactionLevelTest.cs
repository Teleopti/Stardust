using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	public class DonNotReuseTransactionLevelTest : DatabaseTest
	{
		[Test]
		public void TestsetrsetSE()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork(TransactionIsolationLevel.Serializable))
			{
				Console.WriteLine(serverTransactionLevel(uow));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Console.WriteLine(serverTransactionLevel(uow));
			}
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