using NUnit.Framework;
using System;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	[StaffingPerformanceTest]
	[Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_43447)]
	[Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_Step2_44271)]
	[Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_Step3_44331)]
	public class AddOverTimePerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public UpdateStaffingLevelReadModelOnlySkillCombinationResources UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IAddOverTime AddOverTime;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public IStaffingViewModelCreator StaffingViewModelCreator;

		private DateTime[] timeSerie;
		private Guid[] skillIds;
		private IMultiplicatorDefinitionSet oneMulti;

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
				oneMulti = MultiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().First();
			});

		}

		[Test]
		public void ProvideSuggestionsAndApply()
		{
			Now.Is("2016-08-21 07:00");
			
			var overtimePref = new OvertimePreferences
			{
				OvertimeType = oneMulti,
				SelectedSpecificTimePeriod = new TimePeriod(7,0,15,0),
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromHours(5))
				ScheduleTag = new NullScheduleTag(),
			};
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			WithUnitOfWork.Do(uow =>
			{
				var resultModels = AddOverTime.GetSuggestion(new OverTimeSuggestionModel
				{
					SkillIds = skillIds.ToList(),
					TimeSerie = timeSerie,
					OvertimePreferences = overtimePref
				});
				AddOverTime.Apply(resultModels.Models, oneMulti.Id.GetValueOrDefault());
			});

		}
	}

	
}
