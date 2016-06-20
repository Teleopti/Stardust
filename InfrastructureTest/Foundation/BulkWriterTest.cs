using System;
using System.Data;
using System.Data.SqlClient;
using System.Timers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	[Category("LongRunning")]
	public class BulkWriterTest
	{
		private SqlTransaction _transaction;

		[SetUp]
		public void Init()
		{
			_transaction = null;
		}

		[Test]
		public void ShouldWork()
		{
			var writer = new BulkWriter();
			var dt = dim_overtime.CreateTable();
			dt.AddOvertime(1, Guid.NewGuid(), "test", 1, 1, false);
			writer.WriteWithRetries(dt, InfraTestConfigReader.AnalyticsConnectionString, dt.TableName);
		}

		[Test]
		public void ShouldDoRetriesDuringTableLock()
		{
			var writer = new BulkWriter(1);
			var dt = dim_overtime.CreateTable();
			dt.AddOvertime(1, Guid.NewGuid(), "test", 1, 1, false);

			lockTable(dt, InfraTestConfigReader.AnalyticsConnectionString);
			transactionCommitInSeconds(10);
			writer.WriteWithRetries(dt, InfraTestConfigReader.AnalyticsConnectionString, dt.TableName);

			Console.WriteLine(writer.Retries);
			writer.Retries.Should().Be.GreaterThan(2);
		}

		private void transactionCommitInSeconds(int seconds)
		{
			var t = new Timer { Interval = 1000 * seconds };
			t.Elapsed += transactionCommitTimer;
			t.Start();
		}

		private void transactionCommitTimer(object sender, EventArgs e)
		{
			_transaction?.Commit();
		}

		private void lockTable(DataTable dt, string connectionString)
		{
			var connection = new SqlConnection(connectionString);
			connection.Open();
			_transaction = connection.BeginTransaction();

			var command = connection.CreateCommand();
			command.Transaction = _transaction;
			command.CommandText = $"SELECT * FROM {dt.TableName} WITH (TABLOCKX, HOLDLOCK)";
			command.ExecuteNonQuery();
		}
	}
}