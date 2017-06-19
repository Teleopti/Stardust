﻿using System;
using System.Data.SqlClient;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	[TestFixture]
	[UpdateReadModelPerformanceTest]
	public class UpdateStaffingReadModelWithOnlySkillCombinationResourcesTest : PerformanceTestWithOneTimeSetup
	{
		public UpdateStaffingLevelReadModelOnlySkillCombinationResources UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IConfigReader ConfigReader;

		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-26 07:00");
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			
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

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(now.AddDays(-1).AddHours(-1), now.AddDays(14).AddHours(1)));
			});
		}
	}
}