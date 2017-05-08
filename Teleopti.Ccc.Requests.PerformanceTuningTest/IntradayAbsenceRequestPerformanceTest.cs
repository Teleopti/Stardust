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
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx),Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388), Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_43447)]
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
		public void Run200Requests()
		{
			Now.Is("2016-03-16 08:00");

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

		private void onTimedEvent(Object source, ElapsedEventArgs e)
		{
			_nowDateTime = _nowDateTime.AddSeconds(1);
			Now.Is(_nowDateTime);
		}

		[Test, Ignore("WIP")]
		public void Run200RequestsPossibleDeadLock()
		{
			_nowDateTime = new DateTime(2016, 03, 16, 7, 0, 0).Utc();
			Now.Is(_nowDateTime);
			var nowTimer = new Timer(1000);
			nowTimer.Elapsed += onTimedEvent;
			nowTimer.AutoReset = true;
			nowTimer.Enabled = true;
			

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			
			StardustJobFeedback.SendProgress($"Will process {requests.Count} requests");
			var r1 = Task.Factory.StartNew(() =>
											   WithUnitOfWork.Do(() =>
												   {
													   foreach (var request in requests.Take(100))
													   {
														   AbsencePersister.PersistIntradayAbsence(new AddIntradayAbsenceCommand
														   {
															   AbsenceId = ((IAbsenceRequest) request.Request).Absence.Id.GetValueOrDefault(),
															   EndTime = request.Request.Period.EndDateTime,
															   StartTime = request.Request.Period.StartDateTime,
															   PersonId = request.Person.Id.GetValueOrDefault()
														   });
													   }
												   }
											   )
			);

	
			var r2 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				UpdateStaffingLevel.Update(new DateTimePeriod(Now.UtcDateTime().AddDays(-2), Now.UtcDateTime().AddDays(2)));
			}));

			var r3 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				AbsenceRequestIntradayFilter.Process(requests.Reverse().Take(100).First());
			}));
			var r4 = Task.Factory.StartNew(() => WithUnitOfWork.Do(() =>
			{
				AbsenceRequestIntradayFilter.Process(requests.Reverse().Take(100).Second());
			}));

			Task.WaitAll(r1, r2, r3, r4);
			nowTimer.Stop();
			nowTimer.Dispose();
		}
	}
}