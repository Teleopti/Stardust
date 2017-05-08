using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	[Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388)]
	[RequestPerformanceTuningTest]
	public class IntradayAbsenceRequestPerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public FakeConfigReader ConfigReader;
		public IPersonRequestRepository PersonRequestRepository;
		public IAbsenceRequestIntradayFilter AbsenceRequestIntradayFilter;
		public IStardustJobFeedback StardustJobFeedback;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public ISkillRepository SkillRepository;
		public IWorkloadRepository WorkloadRepository;
		public IContractRepository ContractRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IDayOffTemplateRepository DayOffTemplateRepository;
		public IActivityRepository ActivityRepository;

		private IList<IPersonRequest> requests1;
		private IList<IPersonRequest> requests2;

		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-16 07:00");
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

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
			requests1 = new List<IPersonRequest>();
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
								  requests1 = PersonRequestRepository.FindPersonRequestWithinPeriod(new DateTimePeriod(new DateTime(2016, 03, 16, 7, 0, 0).Utc(), new DateTime(2016, 03, 16, 8, 0, 0).Utc()));
								  requests2 = PersonRequestRepository.FindPersonRequestWithinPeriod(new DateTimePeriod(new DateTime(2016, 03, 16, 8, 0, 0).Utc(), new DateTime(2016, 03, 16, 10, 0, 0).Utc()));
								  PersonRepository.FindPeople(requests1.Select(x => x.Person.Id.GetValueOrDefault()));
								  PersonRepository.FindPeople(requests2.Select(x => x.Person.Id.GetValueOrDefault()));
							  });
		}

		[Test]
		public void Run200Requests()
		{
			Now.Is("2016-03-16 07:00");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			var allRequest = requests1.ToList();
			allRequest.AddRange(requests2.ToList());
			StardustJobFeedback.SendProgress($"Will process {allRequest.Count} requests");

			WithUnitOfWork.Do(() =>
							  {
								  foreach (var request in allRequest)
								  {
									  AbsenceRequestIntradayFilter.Process(request);
								  }
							  });
		}

		[Test, Ignore("WIP")]
		public async Task Run200RequestsPossibleOptimisticLocke()
		{
			Now.Is("2016-03-16 07:00");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			StardustJobFeedback.SendProgress($"Will process {requests1.Count} requests");
			StardustJobFeedback.SendProgress($"Will process {requests2.Count} requests");


			var r1 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				foreach (var request in requests1)
				{
					AbsenceRequestIntradayFilter.Process(request);
				}
			}));

			var r2 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				foreach (var request in requests2)
				{
					AbsenceRequestIntradayFilter.Process(request);
				}
			}));
			
			await Task.WhenAll(r1,r2);
		}

		[Test, Ignore("WIP")]
		public async Task Run200RequestsPossibleDeadLock()
		{
			Now.Is("2016-03-16 07:00");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			var allRequest = requests1.ToList();
			allRequest.AddRange(requests2.ToList());
			StardustJobFeedback.SendProgress($"Will process {allRequest.Count} requests");


			var r1 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				foreach (var request in allRequest)
				{
					AbsenceRequestIntradayFilter.Process(request);
				}
			}));

			var r2 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				//extending the period a bit
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
			}));

			await Task.WhenAll(r1, r2);
		}
	}
}