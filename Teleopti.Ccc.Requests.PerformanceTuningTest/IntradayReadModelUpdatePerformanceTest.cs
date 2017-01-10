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
	[Toggle(Toggles.AbsenceRequests_Intraday_UseCascading_41969)]
	public class IntradayReadModelUpdatePerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public UpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IConfigReader ConfigReader;


		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-26 07:00");
			var now = Now.UtcDateTime();
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			//Fill up so need to purge later
			WithUnitOfWork.Do(() =>
							  {
								  using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
								  {
									  connection.Open();

									  using (var command = new SqlCommand(@"truncate table readmodel.SkillCombinationResource", connection))
									  {
										  command.ExecuteNonQuery();
									  }
									  connection.Close();
								  }
								  UpdateStaffingLevel.Update(new DateTimePeriod(now.AddDays(-1).AddHours(-1), now.AddDays(1).AddHours(-1)));
							  });
		}

		[Test]
		public void UpdateReadModel() 
		{
			Now.Is("2016-03-26 07:00");
			var now = Now.UtcDateTime();

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			
			WithUnitOfWork.Do(() =>
							  {
								  UpdateStaffingLevel.Update(new DateTimePeriod(now.AddDays(-1), now.AddDays(1)));
							  });
		}
	}
}