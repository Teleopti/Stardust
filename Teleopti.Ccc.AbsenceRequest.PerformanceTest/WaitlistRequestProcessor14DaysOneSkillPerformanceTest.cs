using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
	public class WaitlistRequestProcessor14DaysOneSkillPerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public FakeConfigReader ConfigReader;
		public IPersonRequestRepository PersonRequestRepository;
		public WaitlistRequestHandler WaitlistRequestHandler;
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

		private IList<IPersonRequest> requests;
		private DateTime _nowDateTime;
		private ICollection<IPerson> _personList;
		private List<IWorkflowControlSet> _wfcs;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;


		public override void OneTimeSetUp()
		{
			_wfcs = new List<IWorkflowControlSet>();
			_nowDateTime = new DateTime(2016, 04, 06, 6, 58, 0).Utc();
			Now.Is(_nowDateTime);
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "PrepareWaitlistedRequest14DaysTest.sql";
				var script = File.ReadAllText(path);

				using (var command = new SqlCommand(script, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}

			var now = Now.UtcDateTime();
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(14));
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
				var reqIds =
					PersonRequestRepository.GetWaitlistRequests(new DateTimePeriod(new DateTime(2016, 04, 06, 8, 0, 0).Utc(),
						new DateTime(2016, 04, 06, 17, 0, 0).Utc()));
				requests = PersonRequestRepository.Find(reqIds);
				_personList = PersonRepository.FindPeople(requests.Select(x => x.Person.Id.GetValueOrDefault()));

				var wfcs = _personList.Select(p => p.WorkflowControlSet).Distinct();
				_wfcs.AddRange(wfcs);

				_wfcs.ForEach(w => w.AbsenceRequestWaitlistEnabled = true);
			});
		}


		[Test,Ignore("Waiting for a fast lane Build")]
		public void RunWaitlistedRequestsOneSkillFor14Days()
		{
			Now.Is("2016-04-06 03:59");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			WaitlistRequestHandler.Handle(new ProcessWaitlistedRequestsEvent
			{
				LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
				LogOnDatasource = "Teleopti WFM"
			});
		}
	}
}
