using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Requests.PerformanceTuningTest;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.ViewSchedule.PerformanceTest
{
	public class ProcessCalculateIntradayIntervalPossibilitiesTestAttribute : RequestPerformanceTuningTestAttribute
	{
		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble<AbsenceStaffingPossibilityCalculator>().For<IAbsenceStaffingPossibilityCalculator>();
			isolate.UseTestDouble<OvertimeStaffingPossibilityCalculator>().For<IOvertimeStaffingPossibilityCalculator>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
	}

	[ProcessCalculateIntradayIntervalPossibilitiesTest]
	public class ProcessCalculateIntradayIntervalPossibilities
	{
		private const string tenantName = "Teleopti WFM";
		private readonly Guid businessUnitId = new Guid("1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B");
		private readonly DateTime baseDate = new DateTime(2016, 03, 16, 07, 00, 00, DateTimeKind.Utc);
		
		public MutableNow Now;
		public FakeConfigReader ConfigReader;

		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository PersonRepository;

		public IPersonRequestRepository PersonRequestRepository;
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IAbsenceRepository AbsenceRepository;
		public ISkillRepository SkillRepository;
		public IWorkloadRepository WorkloadRepository;
		public IContractRepository ContractRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IDayOffTemplateRepository DayOffTemplateRepository;
		public IActivityRepository ActivityRepository;
		
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario CurrentScenario;
		public IIntervalLengthFetcher IntervalLengthFetcher;
		public IUserTimeZone UserTimeZone;
		public AbsenceStaffingPossibilityCalculator AbsenceStaffingPossibilityCalculator;
		public OvertimeStaffingPossibilityCalculator OvertimeStaffingPossibilityCalculator;
		public FakeLoggedOnUser LoggedOnUser;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;

		private LicenseSchema schema;

		[Test]
		public void ShouldProcessCalculationOfAbsencePossibilitiesIntradayForMultipleAgents()
		{
			initialise();
			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			updateStaffingLevel(period);
			var personIds = loadPersonIds();
			WithUnitOfWork.Do(() =>
			{
				var personCount = personIds.Length;
				var elapsedMilliseconds = calculatePossibilitiesByPeriod(personIds, getTodayDateOnlyPeriod(), StaffingPossibilityType.Absence) / personCount;

				Console.WriteLine(
					$"querying possibilities for {personIds.Length} persons,{Environment.NewLine}" +
					$"it takes {elapsedMilliseconds} milliseconds for one person,{Environment.NewLine}");
			});
		}


		[Test]
		public void ShouldProcessCalculationOfAbsencePossibilitiesByPeriodForMultipleAgents()
		{
			initialise();
			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(13));
			updateStaffingLevel(period);
			var dateOnlyPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(13));
			//var personIds = loadPersonIds();
			var personIds = new[] { new Guid("0b4390a8-2128-4550-8d03-a14100f34ea1") };
			WithUnitOfWork.Do(() =>
			{
				var personCount = personIds.Length;
				var elapsedMilliseconds = calculatePossibilitiesByPeriod(personIds, dateOnlyPeriod, StaffingPossibilityType.Absence) / personCount;

				Console.WriteLine(
					$"querying possibilities for {personIds.Length} persons,{Environment.NewLine}" +
					$"it takes {elapsedMilliseconds} milliseconds for one person,{Environment.NewLine}");
			});
		}

		[Test]
		public void ShouldProcessCalculationOfOverTimePossibilitiesByPeriodForMultipleAgents()
		{
			initialise();
			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(13));
			updateStaffingLevel(period);
			var dateOnlyPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(13));
			var personIds = loadPersonIds();
			//var personIds = new[] { new Guid("0b4390a8-2128-4550-8d03-a14100f34ea1") };
			WithUnitOfWork.Do(() =>
			{
				var personCount = personIds.Length;
				var elapsedMilliseconds = calculatePossibilitiesByPeriod(personIds, dateOnlyPeriod, StaffingPossibilityType.Overtime) / personCount;

				Console.WriteLine(
					$"querying possibilities for {personIds.Length} persons,{Environment.NewLine}" +
					$"it takes {elapsedMilliseconds} milliseconds for one person,{Environment.NewLine}");
			});
		}

		private static Guid[] loadPersonIds()
		{
			var path = AppDomain.CurrentDomain.BaseDirectory + "/../../PersonIds.txt";
			var content = File.ReadAllText(path);
			return content.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Take(10).Select(Guid.Parse).ToArray();
		}

		private DateOnlyPeriod getTodayDateOnlyPeriod()
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime(), TimeZoneInfo.Utc);
			return new DateOnly(usersNow).ToDateOnlyPeriod();
		}

		private long calculatePossibilitiesByPeriod(Guid[] personIds, DateOnlyPeriod datePeriod, StaffingPossibilityType staffingPossiblityType)
		{
			long totalElapsedMilliseconds = 0;

			var people = PersonRepository.FindPeople(personIds);
			foreach (var person in people)
			{
				LoggedOnUser.SetFakeLoggedOnUser(person);
				setAbsenceRequestOpenPeriods(person.WorkflowControlSet);
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var possibilities = staffingPossiblityType == StaffingPossibilityType.Absence
					? AbsenceStaffingPossibilityCalculator.CalculateIntradayIntervalPossibilities(person, datePeriod).Models
					: OvertimeStaffingPossibilityCalculator.CalculateIntradayIntervalPossibilities(person, datePeriod, true);
				stopwatch.Stop();
				totalElapsedMilliseconds += stopwatch.ElapsedMilliseconds;
				if (!possibilities.Any())
				{
					Console.WriteLine($"{person.Id.GetValueOrDefault()} no data");
				}
			}

			return totalElapsedMilliseconds;
		}

		private void initialise()
		{
			schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(tenantName, schema);
			logonSystem();
			Now.Is(baseDate);
		}

		private void updateStaffingLevel(DateTimePeriod period)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			WithUnitOfWork.Do(() => { UpdateStaffingLevel.Update(period); });
			stopwatch.Stop();
			Console.WriteLine($"time used for update staffing level {stopwatch.ElapsedMilliseconds} milliseconds");
		}

		private void logonSystem()
		{
			try
			{
				using (DataSource.OnThisThreadUse(tenantName))
				{
					AsSystem.Logon(tenantName, businessUnitId);
				}
			}
			catch (Exception)
			{
				Console.WriteLine($"logon failed using tenantName:{tenantName}");
				throw;
			}
		}

		private static void setAbsenceRequestOpenPeriods(IWorkflowControlSet wfcs)
		{
			foreach (var period in wfcs.AbsenceRequestOpenPeriods)
			{
				period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
				period.StaffingThresholdValidator = new StaffingThresholdValidator();
				period.AbsenceRequestProcess = new GrantAbsenceRequest();
				if (period is AbsenceRequestOpenDatePeriod datePeriod) datePeriod.Period = period.OpenForRequestsPeriod;
			}
		}
	}
}
