using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
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
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;

namespace Teleopti.Ccc.AbsenceRequest.PerformanceTest
{
	[RequestPerformanceTuningTest]
	//public class IntradayAbsenceRequestPerformanceTest : PerformanceTestWithOneTimeSetup
	public class IntradayAbsenceRequestPerformanceTest : PerformanceTestWithOneTimeSetup
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
				StardustJobFeedback.SendProgress($"Will run script");
				var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "Prepare200RequestForIntradayTest.sql";
				var script = File.ReadAllText(path);

				using (var command = new SqlCommand(script, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
				StardustJobFeedback.SendProgress($"Have been running the script");
			}

			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			requests = new List<IPersonRequest>();
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
								  PersonRepository.FindPeople(requests.Select(x => x.Person.Id.GetValueOrDefault()));
								  requests = PersonRequestRepository.FindPersonRequestWithinPeriod(new DateTimePeriod(new DateTime(2016, 03, 16, 7, 0, 0).Utc(), new DateTime(2016, 03, 16, 10, 0, 0).Utc()));
								  
							  });
		}

		[Test,Ignore("Waiting for a fast lane Build")]
		public void Run200RequestsWithoutValidation()
		{
			Now.Is("2016-03-16 07:01");
			
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			StardustJobFeedback.SendProgress($"Will process {requests.Count} requests");

			foreach (var request in requests)
			{
				WithUnitOfWork.Do(() =>
				{
					AbsenceRequestProcessor.Process(request);
				});
			}

		}
	}
}