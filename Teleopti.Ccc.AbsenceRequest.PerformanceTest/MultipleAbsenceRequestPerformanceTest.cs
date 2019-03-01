using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	[Ignore("Waiting for a fast lane Build")]
	//public class MultipleAbsenceRequestPerformanceTest : PerformanceTestWithOneTimeSetup
	public class AbsenceRequestThroughputPerformanceTest : PerformanceTestWithOneTimeSetup
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
		public IAbsenceRequestPersister AbsenceRequestPersister;

		private List<Guid> _personIdList = new List<Guid>();
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
				StardustJobFeedback.SendProgress($"Will run script");
				var script = HelperScripts.ClearExistingAbsencesOnperiod;
				using (var command = new SqlCommand(script, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
				StardustJobFeedback.SendProgress($"Have been running the script");
			}


			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var sql = HelperScripts.PersonWithValidSetupForIntradayRequestOnPeriod;
				using (var command = new SqlCommand(sql, connection))
				{
					var reader = command.ExecuteReader();
					while(reader.Read())
						_personIdList.Add((Guid) reader[0]);
				}
				connection.Close();
				
			}

			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			//requests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				StardustJobFeedback.SendProgress($"Preload for less lazy load");
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
				StardustJobFeedback.SendProgress($"Will update staffing readmodel");
				UpdateStaffingLevel.Update(period);
				StardustJobFeedback.SendProgress($"Done update staffing readmodel");
				//_persons = PersonRepository.l
				
			});
		}

		/// <summary>
		/// To see if the loading of skills is fast enough or not. It should not load all skills, workload for all skills and workload day template
		/// for all the workloads
		/// </summary>
		//[Test,Ignore("Waiting for a fast lane Build")]
		[Test]
		public void Run400RequestsWithBudgetGroupValidation()
		{
			Now.Is("2016-03-16 07:01");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			StardustJobFeedback.SendProgress($"Will process {_personIdList.Count} requests");
			foreach (var i in Enumerable.Range(0, _personIdList.Count -1))
			{
				WithUnitOfWork.Do(() =>
				{
					var startTime = new DateTime(2016, 3, 16, 8, 0, 0, DateTimeKind.Utc);
					var endDateTime = new DateTime(2016,3,16,17,0,0,DateTimeKind.Utc);
					
					AbsenceRequestModel model = new AbsenceRequestModel()
					{
						Period = new DateTimePeriod(startTime,endDateTime),
						PersonId = _personIdList[i],
						Message = "Story79139",
						Subject = "Story79139",
						AbsenceId = new Guid("3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F")
					};
					AbsenceRequestPersister.Persist(model);

				});

			}
		}
	}
}