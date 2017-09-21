using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
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
	[RequestPerformanceTuningTest]
	[Toggle(Toggles.Wfm_Requests_OvertimeRequestHandling_45177),
		Toggle(Toggles.Wfm_Requests_Refactoring_45470)]
	public class OvertimeRequestPerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public UpdateStaffingLevelReadModelOnlySkillCombinationResources UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public FakeConfigReader ConfigReader;
		public IPersonRequestRepository PersonRequestRepository;
		public IStardustJobFeedback StardustJobFeedback;
		public IOvertimeRequestProcessor OvertimeRequestProcessor;

		public ISiteRepository SiteRepository;

		private IList<IPersonRequest> requests;
		private DateTime _nowDateTime;

		public override void OneTimeSetUp()
		{
			_nowDateTime = new DateTime(2016, 01, 16, 7, 0, 0).Utc();
			Now.Is(_nowDateTime);
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "Prepare200OvertimeRequests.sql";
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
								  SiteRepository.LoadAllOrderByName();
								  UpdateStaffingLevel.Update(period);
								  requests = PersonRequestRepository.FindPersonRequestWithinPeriod(new DateTimePeriod(new DateTime(2016, 01, 16, 16, 0, 0).Utc(), new DateTime(2016, 01, 16, 20, 0, 0).Utc()));
							  });
		}

		[Test]
		public void Run200Requests()
		{
			Now.Is("2016-01-16 07:01");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			StardustJobFeedback.SendProgress($"Will process {requests.Count} requests");

			foreach (var request in requests)
			{
				WithUnitOfWork.Do(() =>
				{
					OvertimeRequestProcessor.Process(request, true);
				});
			}
			requests.Count(x => x.IsApproved).Should().Be.GreaterThan(150); //just to have something to catch if big changes are done, locally I get 172 approved
		}
	}
}