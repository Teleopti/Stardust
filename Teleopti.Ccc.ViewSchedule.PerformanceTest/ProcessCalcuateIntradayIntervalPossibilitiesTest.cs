using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Requests.PerformanceTuningTest;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ViewSchedule.PerformanceTest
{
	[TestFixture]
	[RequestPerformanceTuningTest]
	public class ProcessCalcuateIntradayIntervalPossibilities
	{
		private const string tenantName = "Teleopti WFM";
		// BusinesUnit "Telia Sverige"
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

		private IList<IPersonRequest> requests;
		public void setup()
		{
			logonSystem();
			Now.Is("2016-03-16 07:00");

			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "Prepare200RequestForIntradayTest.sql";
				var script = File.ReadAllText(path);

				using (var command = new SqlCommand(script, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}

			var now = Now.UtcDateTime();
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			requests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				WorkflowControlSetRepository.LoadAll();
				AbsenceRepository.LoadAll();
				WorkloadRepository.LoadAll();
				SkillRepository.LoadAllSkills();
				ContractRepository.LoadAll();
				SkillTypeRepository.LoadAll();
				PartTimePercentageRepository.LoadAll();
				ContractScheduleRepository.LoadAllAggregate();
				ActivityRepository.LoadAll();
				DayOffTemplateRepository.LoadAll();

				UpdateStaffingLevel.Update(period);
				requests = PersonRequestRepository.FindPersonRequestWithinPeriod(new DateTimePeriod(new DateTime(2016, 03, 16, 7, 0, 0).Utc(), new DateTime(2016, 03, 16, 10, 0, 0).Utc()));
				PersonRepository.FindPeople(requests.Select(x => x.Person.Id.GetValueOrDefault()));
			});

		}

		[Test]
		public void ShouldProcessMultipleCalculationAbsencePossibilities1000()
		{
			//setup();
			logonSystem();
			var now = new DateTime(2016, 03, 16);
			WithUnitOfWork.Do(() =>
			{
				var person = PersonRepository.Get(new Guid("38E7D01D-7530-4213-8364-A14100F34EA1"));
				var currentUser = new FakeLoggedOnUser(person);
				ICacheableStaffingViewModelCreator cacheableStaffingViewModelCreator = new CacheableStaffingViewModelCreator(StaffingViewModelCreator, currentUser);

				_scheduleStaffingPossibilityCalculator = new ScheduleStaffingPossibilityCalculator(new MutableNow(now), currentUser,
					cacheableStaffingViewModelCreator, ScheduleStorage, CurrentScenario);
				var ret = _scheduleStaffingPossibilityCalculator.CalcuateIntradayAbsenceIntervalPossibilities();
					
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
