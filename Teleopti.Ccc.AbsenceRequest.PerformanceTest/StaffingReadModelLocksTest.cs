using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;

namespace Teleopti.Ccc.AbsenceRequest.PerformanceTest
{
	[RequestPerformanceTuningTest]
	[AllTogglesOn]
	public class StaffingReadModelLocksTest : PerformanceTestWithOneTimeSetup
	{
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public FakeConfigReader ConfigReader;
		public IPersonRequestRepository PersonRequestRepository;
		public IAbsenceRequestProcessor AbsenceRequestProcessor;
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
		public AddOverTime AddOverTime;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;

		private IList<IPersonRequest> _requests;
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
			_requests = new List<IPersonRequest>();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
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
				_requests = PersonRequestRepository.FindPersonRequestWithinPeriod(new DateTimePeriod(new DateTime(2016, 03, 16, 7, 0, 0).Utc(), new DateTime(2016, 03, 16, 10, 0, 0).Utc()));
				PersonRepository.FindPeople(_requests.Select(x => x.Person.Id.GetValueOrDefault()));
			});
		}

		[Test,Ignore("Waiting for a fast lane Build")]
		public void AbsencesWithUpdateReadModelDeadLock()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			_nowDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
			Now.Is(_nowDateTime);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-2).AddHours(1));
			var startDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
			var resolution = 15;
			var periods = new List<DateTime>();
			foreach (var intervalIndex in Enumerable.Range(0, 9))
			{
				periods.Add(startDateTime);
				startDateTime = startDateTime.AddMinutes(resolution);
			}
			var taskList = new List<Task>();
			var r2 = Task.Run(() => WithUnitOfWork.Do(() =>
			{
				StardustJobFeedback.SendProgress($"Starting processing for update readmodel R2");
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
				StardustJobFeedback.SendProgress($"Finised processing for update readmodel R2");
			}));
			foreach (var intervalIndex in Enumerable.Range(0, 9))
			{
				taskList.Add(Task.Run(() =>
				{

					StardustJobFeedback.SendProgress($"Starting processing for period start " + periods[intervalIndex].ToLongDateString());
					foreach (var request in _requests)
					{
						WithUnitOfWork.Do(() =>
						{
							AbsencePersister.PersistIntradayAbsence(new AddIntradayAbsenceCommand
							{
								AbsenceId = ((IAbsenceRequest)request.Request).Absence.Id.GetValueOrDefault(),
								EndTime = periods[intervalIndex].AddMinutes(resolution),
								StartTime = periods[intervalIndex],
								PersonId = request.Person.Id.GetValueOrDefault()
							});
						});
					}
					StardustJobFeedback.SendProgress($"Processing finished for period start " + periods[intervalIndex].ToLongDateString());

				}));
			}


			var r3 = Task.Run(() => WithUnitOfWork.Do(() =>
			{
				StardustJobFeedback.SendProgress($"Starting processing for update readmodel R3");
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(6), Now.UtcDateTime().AddDays(10)));
				StardustJobFeedback.SendProgress($"Finised processing for update readmodel R3");
			}));
			taskList.Add(r3);
			taskList.Add(r2);
			Task.WaitAll(taskList.ToArray());
		}

		//[Test, Ignore("Can not provoke daed locks, but leave the code for the future")]
		//public void AbsencesWithUpdateReadModelAndOvertimeDeadLock()
		//{
		//	var someRequests = _requests.Batch(100);
		//	using (DataSource.OnThisThreadUse("Teleopti WFM"))
		//		AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
		//	_nowDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
		//	Now.Is(_nowDateTime);
		//	var startDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
		//	var resolution = 15;
		//	var periods = new List<DateTime>();
		//	foreach (var intervalIndex in Enumerable.Range(0, 9))
		//	{
		//		periods.Add(startDateTime);
		//		startDateTime = startDateTime.AddMinutes(resolution);
		//	}
		//	var taskList = new List<Task>();

		//	var r2 = Task.Run(() => WithUnitOfWork.Do(() =>
		//	{
		//		StardustJobFeedback.SendProgress($"Starting processing for update readmodel R2");
		//		UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
		//		StardustJobFeedback.SendProgress($"Finised processing for update readmodel R2");
		//	}));

		//	foreach (var intervalIndex in Enumerable.Range(0, 9))
		//	{
		//		taskList.Add(Task.Run(() =>
		//		{
		//			WithUnitOfWork.Do(() =>
		//			{
		//				StardustJobFeedback.SendProgress($"Starting processing for period start " + periods[intervalIndex].ToLongDateString());
		//				foreach (var request in someRequests[0])
		//				{
		//					AbsencePersister.PersistIntradayAbsence(new AddIntradayAbsenceCommand
		//					{
		//						AbsenceId = ((IAbsenceRequest)request.Request).Absence.Id.GetValueOrDefault(),
		//						EndTime = periods[intervalIndex].AddMinutes(resolution),
		//						StartTime = periods[intervalIndex],
		//						PersonId = request.Person.Id.GetValueOrDefault()
		//					});
		//				}
		//				StardustJobFeedback.SendProgress($"Processing finished for period start " + periods[intervalIndex].ToLongDateString());
		//			});
		//		}));
		//	}

		//	var overtimeStartDateTime = new DateTime(2016, 03, 16, 17, 0, 0).Utc();
		//	foreach (var intervalIndex in Enumerable.Range(0, 9))
		//	{
		//		periods.Add(startDateTime);
		//		startDateTime = overtimeStartDateTime.AddMinutes(resolution);
		//	}
		//	foreach (var intervalIndex in Enumerable.Range(0, 9))
		//	{
		//		taskList.Add(Task.Run(() =>
		//		{
		//			WithUnitOfWork.Do(() =>
		//			{
		//				IList<OverTimeModel> overTimeModels = new List<OverTimeModel>();
		//				foreach (var request in someRequests[1])
		//				{

		//					new OverTimeModel()
		//					{
		//						StartDateTime = periods[intervalIndex],
		//						EndDateTime = periods[intervalIndex].AddMinutes(15),
		//						ActivityId = ActivityRepository.LoadAll().First().Id.Value,
		//						PersonId = request.Person.Id.Value

		//					};

		//				}
		//				//if not ignoring might need a "real" multiId
		//				AddOverTime.Apply(overTimeModels, Guid.NewGuid());
		//			});
		//		}));
		//	}


		//	var r3 = Task.Run(() => WithUnitOfWork.Do(() =>
		//	{
		//		StardustJobFeedback.SendProgress($"Starting processing for update readmodel R3");
		//		UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
		//		StardustJobFeedback.SendProgress($"Finised processing for update readmodel R3");
		//	}));

		//	taskList.Add(r3);
		//	taskList.Add(r2);
		//	Task.WaitAll(taskList.ToArray());
		//}

		//[Test, Ignore("Can not provoke daed locks, but leave the code for the future")]
		//public void AbsencesWithUpdateReadModelAndAbsenceRequestDeadLock()
		//{
		//	using (DataSource.OnThisThreadUse("Teleopti WFM"))
		//		AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
		//	_nowDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
		//	Now.Is(_nowDateTime);
		//	var startDateTime = new DateTime(2016, 03, 17, 7, 0, 0).Utc();
		//	var resolution = 15;
		//	var periods = new List<DateTime>();
		//	foreach (var intervalIndex in Enumerable.Range(0, 9))
		//	{
		//		periods.Add(startDateTime);
		//		startDateTime = startDateTime.AddMinutes(resolution);
		//	}
		//	var taskList = new List<Task>();
		//	var r2 = Task.Run(() => WithUnitOfWork.Do(() =>
		//	{
		//		UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
		//	}));

		//	IEnumerable<IPerson> allPersons = new List<IPerson>();
		//	WithUnitOfWork.Do(() =>
		//	{
		//		allPersons = PersonRepository.LoadAll();
		//	});
		//	var absenceId = new Guid("5B859CEF-0F35-4BA8-A82E-A14600EEE42E");
		//	foreach (var intervalIndex in Enumerable.Range(0, 9))
		//	{
		//		taskList.Add(Task.Run(() =>
		//		{
		//			WithUnitOfWork.Do(() =>
		//			{
		//				foreach (var person in allPersons)
		//				{
		//					AbsencePersister.PersistIntradayAbsence(new AddIntradayAbsenceCommand
		//					{
		//						AbsenceId = absenceId,
		//						EndTime = periods[intervalIndex].AddMinutes(resolution),
		//						StartTime = periods[intervalIndex],
		//						PersonId = person.Id.GetValueOrDefault()
		//					});
		//				}
		//			});
		//		}));
		//	}

		//	taskList.Add(Task.Run(() =>
		//	{
		//		foreach (var request in _requests)
		//		{
		//			WithUnitOfWork.Do(() =>
		//			{
		//				AbsenceRequestProcessor.Process(request);
		//			});
		//		}
		//	}));

		//	var r4 = Task.Run(() => WithUnitOfWork.Do(() =>
		//	{
		//		UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
		//	}));

		//	var r3 = Task.Run(() => WithUnitOfWork.Do(() =>
		//	{
		//		UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
		//	}));
		//	taskList.Add(r3);
		//	taskList.Add(r2);
		//	taskList.Add(r4);
		//	Task.WaitAll(taskList.ToArray());
		//}

	}
}