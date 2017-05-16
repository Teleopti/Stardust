using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
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
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx),Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388), Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_43447), Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_Step2_44271)]
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
		public IAbsencePersister AbsencePersister;

		private IList<IPersonRequest> requests;
		private DateTime _nowDateTime;

		public override void OneTimeSetUp()
		{
			_nowDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
			Now.Is(_nowDateTime);
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
			requests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
							  {
								  WorkflowControlSetRepository.LoadAll();
								  AbsenceRepository.LoadAll();
								  WorkloadRepository.LoadAll();
								  ActivityRepository.LoadAll();
								  SkillTypeRepository.LoadAll();
								  SkillRepository.LoadAllSkills();
								  ContractRepository.LoadAll();
								  PartTimePercentageRepository.LoadAll();
								  ContractScheduleRepository.LoadAllAggregate();
								  DayOffTemplateRepository.LoadAll();

								  UpdateStaffingLevel.Update(period);
								  requests = PersonRequestRepository.FindPersonRequestWithinPeriod(new DateTimePeriod(new DateTime(2016, 03, 16, 7, 0, 0).Utc(), new DateTime(2016, 03, 16, 10, 0, 0).Utc()));
								  PersonRepository.FindPeople(requests.Select(x => x.Person.Id.GetValueOrDefault()));
							  });
		}

		[Test]
		public void Run200Requests()
		{
			Now.Is("2016-03-16 07:01");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			StardustJobFeedback.SendProgress($"Will process {requests.Count} requests");

			WithUnitOfWork.Do(() =>
							  {
								  foreach (var request in requests)
								  {
									  AbsenceRequestIntradayFilter.Process(request);
								  }
							  });
		}

		[Test]
		public void AbsencesWithUpdateReadModelDeadLock()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			_nowDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
			Now.Is(_nowDateTime);
			var startDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
			var resolution = 15;
			var periods = new List<DateTime>();
			foreach (var intervalIndex in Enumerable.Range(0, 9))
			{
				periods.Add(startDateTime);
				startDateTime = startDateTime.AddMinutes(resolution);
			}
			var taskList = new List<Task>();
			var r2 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
			}));
			foreach (var intervalIndex in Enumerable.Range(0,9))
			{
				taskList.Add(Task.Factory.StartNew(() =>
				{
					WithUnitOfWork.Do(() =>
					{
						foreach (var request in requests)
						{
							AbsencePersister.PersistIntradayAbsence(new AddIntradayAbsenceCommand
							{
								AbsenceId = ((IAbsenceRequest)request.Request).Absence.Id.GetValueOrDefault(),
								EndTime = periods[intervalIndex].AddMinutes(resolution),
								StartTime = periods[intervalIndex],
								PersonId = request.Person.Id.GetValueOrDefault()
							});
						}
					});
				}));
			}
			

			var r3 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
			}));
			taskList.Add(r3);
			taskList.Add(r2);
			Task.WaitAll(taskList.ToArray());
		}

		[Test, Ignore("Can not provoke daed locks, but leave the code for the future")]
		public void AbsencesWithUpdateReadModelAndAbsenceRequestDeadLock()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			_nowDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
			Now.Is(_nowDateTime);
			var startDateTime = new DateTime(2016, 03, 17, 7, 0, 0).Utc();
			var resolution = 15;
			var periods = new List<DateTime>();
			foreach (var intervalIndex in Enumerable.Range(0, 9))
			{
				periods.Add(startDateTime);
				startDateTime = startDateTime.AddMinutes(resolution);
			}
			var taskList = new List<Task>();
			var r2 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
			}));

			IList<IPerson> allPersons = new List<IPerson>();
			WithUnitOfWork.Do(() =>
			{
				allPersons = PersonRepository.LoadAll();
			});
			var absenceId = new Guid("5B859CEF-0F35-4BA8-A82E-A14600EEE42E");
			foreach (var intervalIndex in Enumerable.Range(0, 9))
			{
				taskList.Add(Task.Factory.StartNew(() =>
				{
					WithUnitOfWork.Do(() =>
					{
						foreach (var person in allPersons)
						{
							AbsencePersister.PersistIntradayAbsence(new AddIntradayAbsenceCommand
							{
								AbsenceId = absenceId,
								EndTime = periods[intervalIndex].AddMinutes(resolution),
								StartTime = periods[intervalIndex],
								PersonId = person.Id.GetValueOrDefault()
							});
						}
					});
				}));
			}

			taskList.Add(Task.Factory.StartNew(() =>
			{
				foreach (var request in requests)
				{
					WithUnitOfWork.Do(() =>
					{
						AbsenceRequestIntradayFilter.Process(request);
					});
				}
			}));

			var r4 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
			}));

			var r3 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
			}));
			taskList.Add(r3);
			taskList.Add(r2);
			taskList.Add(r4);
			Task.WaitAll(taskList.ToArray());
		}

	}
}