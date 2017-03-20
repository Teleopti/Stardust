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
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Requests.PerformanceTuningTest;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ViewSchedule.PerformanceTest
{
	[TestFixture]
	[RequestPerformanceTuningTest]
	public class ProcessCalcuateIntradayIntervalPossibilities : ISetup
	{
		private const string tenantName = "Teleopti WFM";
		private readonly Guid businessUnitId = new Guid("1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B");

		private ScheduleStaffingPossibilityCalculator _scheduleStaffingPossibilityCalculator;

		public MutableNow Now;
		public FakeConfigReader ConfigReader;

		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository PersonRepository;

		public IPersonRequestRepository PersonRequestRepository;
		public UpdateStaffingLevelReadModel UpdateStaffingLevel;
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

		public IStaffingViewModelCreator StaffingViewModelCreator;
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario CurrentScenario;
		public IIntervalLengthFetcher IntervalLengthFetcher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			var fakeIntervalLengthFetcher = new FakeIntervalLengthFetcher();
			fakeIntervalLengthFetcher.Has(15);
			system.UseTestDouble(fakeIntervalLengthFetcher).For<IIntervalLengthFetcher>();
		}

		[Test]
		public void ShouldProcessMultipleCalculationAbsencePossibilities1000()
		{
			setup();
			var personIds = loadPersonIds();
			WithUnitOfWork.Do(() =>
			{
				var personCount = personIds.Length;
				var firstTimeElapsedMilliseconds = calcuatePossibilities(personIds)/personCount;
				var secondTimeElapsedMilliseconds = calcuatePossibilities(personIds)/personCount;

				Console.WriteLine(
					$"querying possibilities for {personIds.Length} persons,{Environment.NewLine}" +
					$"the first time it takes {firstTimeElapsedMilliseconds} milliseconds for one person,{Environment.NewLine}" +
					$"and the second time it takes {secondTimeElapsedMilliseconds} milliseconds for one person");
			});
		}

		private static Guid[] loadPersonIds()
		{
			var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "PersonIds.txt";
			var content = File.ReadAllText(path);
			return content.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToArray();
		}

		private long calcuatePossibilities(Guid[] personIds)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var people = PersonRepository.FindPeople(personIds);
			foreach (var person in people)
			{
				var currentUser = new FakeLoggedOnUser(person);
				var cacheableStaffingViewModelCreator = new CacheableStaffingViewModelCreator(StaffingViewModelCreator,
					IntervalLengthFetcher);
				_scheduleStaffingPossibilityCalculator = new ScheduleStaffingPossibilityCalculator(Now, currentUser,
					cacheableStaffingViewModelCreator, ScheduleStorage, CurrentScenario);
				var possibilities = _scheduleStaffingPossibilityCalculator.CalcuateIntradayAbsenceIntervalPossibilities();
				if (!possibilities.Any())
				{
					Console.WriteLine($"{person.Id.GetValueOrDefault()} no data");
				}
			}
			stopwatch.Stop();
			return stopwatch.ElapsedMilliseconds;
		}

		private void setup()
		{
			logonSystem();
			Now.Is("2016-03-16 07:00");
			var now = Now.UtcDateTime();
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(period);
			});
		}

		private void logonSystem()
		{
			using (DataSource.OnThisThreadUse(tenantName))
			{
				AsSystem.Logon(tenantName, businessUnitId);
			}
		}
	}
}
