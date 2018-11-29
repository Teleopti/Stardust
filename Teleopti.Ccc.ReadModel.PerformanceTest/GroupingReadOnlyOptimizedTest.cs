using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	[TestFixture]
	[PerformanceTest]
	[Category("GroupingReadOnlyOptimizedTest")]
	public class GroupingReadOnlyOptimizedTest
	{
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public IGroupingReadOnlyRepository Target;
		public ImpersonateSystem Impersonate;
		public TestLog TestLog;
		public IBusinessUnitRepository BusinessUnits;
		public AsSystem AsSystem;

		[Test]
		public void MeasurePerformance()
		{
			IEnumerable<ReadOnlyGroupDetail> details;

			var logOnDatasource = "TestData";
			var businessUnitId = getBussinessUnitId(logOnDatasource);

			Impersonate.Impersonate(logOnDatasource, businessUnitId);

			var sw = new Stopwatch();
			sw.Start();

			details = WithUnitOfWork.Get(() => Target.AvailableGroups(new DateOnly(DateTime.UtcNow)));

			sw.Stop();

			TestLog.Debug($"It costed {sw.ElapsedMilliseconds} milliseconds to get {details.Count()} AvailableGroups from GroupingReadOnly.");
		}

		[Test]
		public void MeasureLoadAvalibaleGroupPerformance()
		{
			var logOnDatasource = "TestData";
			var businessUnitId = getBussinessUnitId(logOnDatasource);

			Impersonate.Impersonate(logOnDatasource, businessUnitId);

			var sw = new Stopwatch();
			sw.Start();

			var groups = WithUnitOfWork.Get(() => Target.AllAvailableGroups(new DateOnlyPeriod(new DateOnly(2012, 12, 31), new DateOnly(2018, 12, 8))));

			sw.Stop();
			TestLog.Debug($"It costed {sw.ElapsedMilliseconds} milliseconds to get {groups.Count()} distinct  AvailableGroups");
		}

		public Guid getBussinessUnitId(string logOnDatasource) {
		
			Guid businessUnitId;
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
			return businessUnitId;
		}

	}
}
