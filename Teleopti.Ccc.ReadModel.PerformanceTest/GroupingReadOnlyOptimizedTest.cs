using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	[TestFixture]
	[PerformanceTest]
	[Category("GroupingReadOnlyOptimizedTest")]
	public class GroupingReadOnlyOptimizedTest
	{
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public IGroupingReadOnlyRepository GroupingReadOnlyUnits;
		public ImpersonateSystem Impersonate;
		public TestLog TestLog;

		[Test]
		public void MeasurePerformance()
		{
			IEnumerable<ReadOnlyGroupDetail> details;
			Guid businessUnitId;
			const string logOnDatasource = "TestData";

			using (DataSource.OnThisThreadUse(logOnDatasource))
			{
				using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
				{
					connection.Open();
					var script = "SELECT TOP(1) BusinessUnitId FROM ReadModel.GroupingReadOnly GROUP BY BusinessUnitId ORDER BY COUNT(BusinessUnitId) DESC"; // for getting the BusinessUnitId with most AvailableGroups

					using (var command = new SqlCommand(script, connection))
					{
						businessUnitId = Guid.Parse(command.ExecuteScalar().ToString());
					}
					connection.Close();
				}
			}

			Impersonate.Impersonate(logOnDatasource, businessUnitId);

			var sw = new Stopwatch();
			sw.Start();

			details = WithUnitOfWork.Get(() => GroupingReadOnlyUnits.AvailableGroups(new DateOnly(DateTime.UtcNow)));

			sw.Stop();

			TestLog.Debug($"It costed {sw.ElapsedMilliseconds} milliseconds to get {details.Count()} AvailableGroups from GroupingReadOnly.");
		}
	}
}
