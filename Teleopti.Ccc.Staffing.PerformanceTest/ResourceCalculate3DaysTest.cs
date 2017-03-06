using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	[StaffingPerformanceTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_42663)]
	//[Toggle(Toggles.StaffingActions_UseRealForecast_42663)]
	public class ResourceCalculate3DaysTest : PerformanceTestWithOneTimeSetup
	{
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IConfigReader ConfigReader;
		public ISkillStaffingIntervalProvider SkillStaffingIntervalProvider;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;

		private IEnumerable<SkillCombinationResource> skillCombinationResources;

		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-07 07:00");
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			var now = Now.UtcDateTime();
			var period = new DateTimePeriod(now.AddHours(-1), now.AddDays(3));
			WithUnitOfWork.Do(() =>
			{

				using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
				{
					connection.Open();

					using (var command = new SqlCommand(@"truncate table readmodel.SkillCombinationResource", connection))
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
					using (var command = new SqlCommand(@"truncate table readmodel.ScheduleForecastSkill", connection))
					{
						command.ExecuteNonQuery();
					}
					using (var command = new SqlCommand(@"truncate table readmodel.ScheduleForecastSkillChange", connection))
					{
						command.ExecuteNonQuery();
					}
				}
				UpdateStaffingLevel.Update(period);
			});
		}

		[Test]
		public void ResourceCalculate3DaysWithShrinkage()
		{
			Now.Is("2016-03-07 07:00");
			var now = Now.UtcDateTime();
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			var period = new DateTimePeriod(now, now.AddDays(3));
			WithUnitOfWork.Do(() =>
			{
				skillCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period);
				SkillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, skillCombinationResources.ToList(), true); //with shrinkage
			}); 
		}

	}
}
