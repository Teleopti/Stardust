using System;
using System.Data.SqlClient;
using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	[TestFixture]
	[RequestPerformanceTest]
	[Explicit]
	[Toggle(Toggles.AbsenceRequests_Intraday_UseCascading_41969)]
	public class IntradayAbsenceRequestPerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public UpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IConfigReader ConfigReader;


		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-16");
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var script = File.ReadAllText("C:/Development/teleopticcc/Teleopti.Ccc.Requests.PerformanceTuningTest/Prepare200RequestForIntradayTest.sql");
			
						using (var command = new SqlCommand(script, connection))
						{
							command.ExecuteNonQuery();
						}
				connection.Close();
			}
			
			var now = Now.UtcDateTime().Date;
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			WithUnitOfWork.Do(() =>
							  {
								  UpdateStaffingLevel.Update(period);
							  });
			
		}

		[Test]
		public void DoTheThing()
		{
			Console.WriteLine("bump");
			Assert.Pass();
		}
	}
}