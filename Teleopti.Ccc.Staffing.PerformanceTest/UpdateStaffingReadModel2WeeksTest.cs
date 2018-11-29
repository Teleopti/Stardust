using System;
using System.Data.SqlClient;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;


namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	[TestFixture]
	[UpdateReadModelPerformanceTest]
	public class UpdateStaffingReadModel2WeeksTest : PerformanceTestWithOneTimeSetup
	{
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IConfigReader ConfigReader;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;

		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-26 07:00");
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			WithUnitOfWork.Do(() =>
			{
				using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
				{
					connection.Open();

					using (var command = new SqlCommand(@"truncate table readmodel.SkillCombination", connection))
					{
						command.ExecuteNonQuery();
					}
					using (var command = new SqlCommand(@"truncate table readmodel.SkillCombinationResourceDelta", connection))
					{
						command.ExecuteNonQuery();
					}
					using (var command = new SqlCommand(@"truncate table readmodel.SkillCombinationResource", connection))
					{
						command.ExecuteNonQuery();
					}
				}
			});
		}

		[Test]
		public void UpdateReadModel2Weeks()
		{
			Now.Is("2016-03-26 07:00");
			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(now.AddDays(-1).AddHours(-1), now.AddDays(14).AddHours(1)));
			});
		}

		[Test]
		public void UpdateReadModel4Weeks()
		{
			Now.Is("2016-03-26 07:00");
			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(now.AddDays(-1).AddHours(-1), now.AddDays(28).AddHours(1)));
			});
		}

		[Test]
		public void UpdateReadModel7Weeks()
		{
			Now.Is("2016-03-26 07:00");
			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(now.AddDays(-1).AddHours(-1), now.AddDays(49).AddHours(1)));
			});
		}
	}
}