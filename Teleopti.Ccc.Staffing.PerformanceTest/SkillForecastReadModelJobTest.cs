using System;
using System.Data.SqlClient;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	[StaffingPerformanceTest]
	[AllTogglesOn]
	public class SkillForecastReadModelJobTest : PerformanceTestWithOneTimeSetup
	{
		public UpdateSkillForecastReadModelHandler UpdateSkillForecastReadModelHandler;
		public SkillForecastReadModelPeriodBuilder SkillForecastReadModelPeriodBuilder;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public FakeConfigReader ConfigReader;
		public AddOverTime AddOverTime;
		private DateTime _nowDateTime;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;


		public override void OneTimeSetUp()
		{
			_nowDateTime = new DateTime(2013, 01, 12, 10, 0, 0).Utc();
			Now.Is(_nowDateTime);
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

	
			WithUnitOfWork.Do(() =>
			{
				using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
				{
					connection.Open();

					using (var command = new SqlCommand(@"truncate table readmodel.SkillForecast", connection))
					{
						command.ExecuteNonQuery();
					}
					using (var command = new SqlCommand(@"truncate table SkillForecastJobStartTime", connection))
					{
						command.ExecuteNonQuery();
					}
				}
			});
		}

		[Test]
		public void CalculateSkillForecastIntervalsForSixMonths()
		{
			_nowDateTime = new DateTime(2016, 01, 12, 12, 0, 0).Utc();
			Now.Is(_nowDateTime);

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			WithUnitOfWork.Do(() =>
			{
				UpdateSkillForecastReadModelHandler.Handle(new UpdateSkillForecastReadModelEvent()
				{
					StartDateTime = new DateTime(2016,01,01),
					EndDateTime = new DateTime(2016, 06, 01),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM"
				});
			});
		}
	}
}
