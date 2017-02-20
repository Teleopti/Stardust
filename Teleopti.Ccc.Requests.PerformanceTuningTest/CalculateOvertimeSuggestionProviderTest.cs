﻿using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	[RequestPerformanceTuningTest]
	[Toggle(Toggles.AbsenceRequests_Intraday_UseCascading_41969)]
	[Toggle(Toggles.StaffingActions_UseRealForecast_42663)]
	[Toggle(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_42663)]
	[Ignore("WIP")]
	public class AddOverTimePerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IAddOverTime AddOverTime;

		public IStaffingViewModelCreator StaffingViewModelCreator;

		private DateTime[] timeSerie;
		private Guid[] skillIds;

		public override void OneTimeSetUp()
		{
			Now.Is("2016-08-21 07:00");
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			skillIds = new[] {new Guid("0165E0EA-210A-4393-B25A-A15000925656")};
			var now = Now.UtcDateTime();
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			//var period = new DateTimePeriod(now, now.AddHours(24));
			WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(period);

				var viewModel = StaffingViewModelCreator.Load(skillIds);
				timeSerie = viewModel.DataSeries.Time;
			});

		}

		[Test]
		public void ShouldProvideCalculatedSuggestions()
		{
			Now.Is("2016-08-21 07:00");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			WithUnitOfWork.Do(() =>
			{
				//var startDateTime = new DateTime(2016, 08, 21, 15, 0, 0);
				//var endDateTime = new DateTime(2016, 08, 21, 18, 0, 0);
				var resultModel = AddOverTime.GetSuggestion(new OverTimeSuggestionModel
										  {
											  SkillIds = skillIds.ToList(),
											  TimeSerie = timeSerie
										  });
				AddOverTime.Apply(resultModel.OverTimeModels);
			});

		}
	}

	
}
