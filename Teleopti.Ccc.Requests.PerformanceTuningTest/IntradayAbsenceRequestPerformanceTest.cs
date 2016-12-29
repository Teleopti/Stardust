using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	[TestFixture]
	[RequestPerformanceTest]
	[Explicit]
	[Toggle(Toggles.AbsenceRequests_Intraday_UseCascading_41969)]
	public class IntradayAbsenceRequestPerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public UpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IConfigReader ConfigReader;
		public IPersonRequestRepository PersonRequestRepository;
		public IAbsenceRequestIntradayFilter AbsenceRequestIntradayFilter;
		public IStardustJobFeedback StardustJobFeedback;

		private IList<IPersonRequest> requests;

		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-16 07:00");
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "Prepare500RequestForIntradayTest.sql";
				var script = File.ReadAllText(path);
			
						using (var command = new SqlCommand(script, connection))
						{
							command.ExecuteNonQuery();
						}
				connection.Close();
			}
			
			var now = Now.UtcDateTime().Date;
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			WithUnitOfWork.Do(() =>
							  {
								  UpdateStaffingLevel.Update(period);
								  var allRequests = PersonRequestRepository.LoadAll();
								  requests = allRequests.Where(x => x.RequestedDate.Equals(new DateTime(2016, 03, 16))).ToList();
							  });
			//var allRequests = WithUnitOfWork.Get(() => );
		}

		[Test]
		public void DoTheThing()
		{
			StardustJobFeedback.SendProgress($"Will process {requests.Count} requests");
			foreach (var request in requests)
			{
				AbsenceRequestIntradayFilter.Process(request);
			}
		}
	}
}