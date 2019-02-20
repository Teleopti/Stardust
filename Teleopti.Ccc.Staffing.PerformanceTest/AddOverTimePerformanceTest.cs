using NUnit.Framework;
using System;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	[StaffingPerformanceTest]
	[AllTogglesOn]
	public class AddOverTimePerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public AddOverTime AddOverTime;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public IStaffingViewModelCreator StaffingViewModelCreator;
		public UpdateSkillForecastReadModel UpdateSkillForecastReadModel;
		public IConfigReader ConfigReader;

		private DateTime[] timeSerie;
		private Guid[] skillIds;
		private IMultiplicatorDefinitionSet oneMulti;
		private DateTime now = new DateTime(2016, 08, 10, 7, 0, 0).Utc();
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;

		public override void OneTimeSetUp()
		{
			Now.Is(now);
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			skillIds = new[] {new Guid("0165E0EA-210A-4393-B25A-A15000925656")};
			var period = new DateTimePeriod(now.AddDays(-2), now.AddDays(2));
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
					using (var command = new SqlCommand(@"truncate table readmodel.SkillForecast", connection))
					{
						command.ExecuteNonQuery();
					}
				}

				UpdateStaffingLevel.Update(period);
				UpdateSkillForecastReadModel.Update(period);

				var viewModel = StaffingViewModelCreator.Load(skillIds);
				timeSerie = viewModel.DataSeries.Time;
				oneMulti = MultiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().First();
			});

		}

		[Test]
		public void ProvideSuggestionsAndApplyOvertime()
		{
			Now.Is(now);
			
			var overtimePref = new OvertimePreferences
			{
				OvertimeType = oneMulti,
				SelectedSpecificTimePeriod = new TimePeriod(7,0,15,0),
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromHours(5)),
				ScheduleTag = new NullScheduleTag(),
			};
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			WithUnitOfWork.Do(uow =>
			{
				Console.WriteLine($"{DateTime.Now} Start GetSuggestion()");
				var resultModels = AddOverTime.GetSuggestion(new OverTimeSuggestionModel
				{
					SkillIds = skillIds.ToList(),
					TimeSerie = timeSerie,
					OvertimePreferences = overtimePref,
					NumberOfPersonsToTry = 1000
				});
				Console.WriteLine($"{DateTime.Now} GetSuggestion() gave {resultModels.Models.Count} models");
				AddOverTime.Apply(resultModels.Models, oneMulti.Id.GetValueOrDefault());
				Console.WriteLine($"{DateTime.Now} Done!");
			});

		}
	}

	
}
