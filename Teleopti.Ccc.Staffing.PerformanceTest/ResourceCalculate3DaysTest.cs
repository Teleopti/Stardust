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
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	[StaffingPerformanceTest]
	[AllTogglesOn]
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
		public ScheduledStaffingViewModelCreator StaffingViewModelCreator;
		public ISkillRepository SkillRepository;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;

		private IEnumerable<ISkill> skills;

		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-07 07:00");
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			var now = Now.UtcDateTime();
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(4));
			var deltas = new List<SkillCombinationResource>();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			WithUnitOfWork.Do((uow) =>
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
				skills = SkillRepository.LoadAllSkills();
				UpdateStaffingLevel.Update(period);
				uow.Current().PersistAll();
				var skillCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period);
				foreach (var skillCombinationResource in skillCombinationResources)
				{
					for (var index = 1; index <= 10; index++)
					{
						deltas.Add(new SkillCombinationResource
						{
							StartDateTime = skillCombinationResource.StartDateTime,
							EndDateTime = skillCombinationResource.EndDateTime,
							SkillCombination = skillCombinationResource.SkillCombination,
							Resource = skillCombinationResource.Resource / index
						});
					}
				}
				SkillCombinationResourceRepository.PersistChanges(deltas);
			});
		}

		//[Test]
		//public void ResourceCalculate3DaysWithShrinkage()
		//{
		//	Now.Is("2016-03-07 07:00");
		//	var now = Now.UtcDateTime();
		//	using (DataSource.OnThisThreadUse("Teleopti WFM"))
		//		AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

		//	var period = new DateTimePeriod(now, now.AddDays(3));
		//	WithUnitOfWork.Do(() =>
		//					  {
		//						  var skillCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period);
		//						  var result = SkillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, skillCombinationResources.ToList(), true); //with shrinkage
		//						  Assert.Greater(result.Count, 0);
		//					  });
		//}

		//[Test]
		//public void ResourceCalculate3DaysWithoutShrinkage()
		//{
		//	Now.Is("2016-03-07 07:00");
		//	var now = Now.UtcDateTime();
		//	using (DataSource.OnThisThreadUse("Teleopti WFM"))
		//		AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

		//	var period = new DateTimePeriod(now, now.AddDays(3));
		//	WithUnitOfWork.Do(() =>
		//					  {
		//						  var skillCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period);
		//						  var result = SkillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, skillCombinationResources.ToList(), false);
		//						  Assert.Greater(result.Count, 0);
		//					  });
		//}



		//[Test]
		//public void GetForecastAndStaffingInfoForTodayWithoutShrinkage()
		//{
		//	Now.Is("2016-03-07 07:00");
		//	using (DataSource.OnThisThreadUse("Teleopti WFM"))
		//		AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
		//	WithUnitOfWork.Do(() =>
		//					  {
		//						  var result = StaffingViewModelCreator.Load(skills.Select(x => x.Id.GetValueOrDefault()).ToArray());
		//						  Assert.AreEqual(result.StaffingHasData, true);
		//						  Assert.Greater(result.DataSeries.Time.Length, 0);
		//						  Assert.Greater(result.DataSeries.ForecastedStaffing.Length, 0);
		//						  Assert.Greater(result.DataSeries.ScheduledStaffing.Length, 0);
		//					  });
		//}

		//[Test]
		//public void GetForecastAndStaffingInfoForTodayWithShrinkage()
		//{
		//	Now.Is("2016-03-07 07:00");
		//	using (DataSource.OnThisThreadUse("Teleopti WFM"))
		//		AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
		//	WithUnitOfWork.Do(() =>
		//					  {
		//						  var result = StaffingViewModelCreator.Load(skills.Select(x => x.Id.GetValueOrDefault()).ToArray(), null, true);
		//						  Assert.AreEqual(result.StaffingHasData, true);
		//						  Assert.Greater(result.DataSeries.Time.Length, 0);
		//						  Assert.Greater(result.DataSeries.ForecastedStaffing.Length, 0);
		//						  Assert.Greater(result.DataSeries.ScheduledStaffing.Length, 0);
		//					  });
		//}

	}
}
