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
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{

	[RequestPerformanceTuningTest]
	public class WaitlistRequestWaitlistPositionPerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public FakeConfigReader ConfigReader;
		public IPersonRequestRepository PersonRequestRepository;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public IAbsenceRequestWaitlistProvider AbsenceRequestWaitlistProvider;

		private IList<IPersonRequest> _requests;
		private DateTime _nowDateTime;

		private const string tenantName = "Teleopti WFM";
		private readonly Guid businessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b");

		public override void OneTimeSetUp()
		{
			var schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(tenantName, schema);

			_nowDateTime = new DateTime(2016, 04, 06, 6, 58, 0).Utc();
			Now.Is(_nowDateTime);

			using (DataSource.OnThisThreadUse(tenantName))
				AsSystem.Logon(tenantName, businessUnitId);

			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "Prepare5000WaitlistedRequestForWaitlistPositionTest.sql";
				var script = File.ReadAllText(path);

				using (var command = new SqlCommand(script, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}

			_requests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				WorkflowControlSetRepository.LoadAll();
				AbsenceRepository.LoadAll();

				var reqIds =
					PersonRequestRepository.GetWaitlistRequests(new DateTimePeriod(new DateTime(2016, 04, 06, 8, 0, 0).Utc(),
						new DateTime(2016, 04, 06, 17, 0, 0).Utc()));
				_requests = PersonRequestRepository.FindPersonReuqestsWithAbsenceAndPersonPeriods(reqIds).Where(p => p.Person.WorkflowControlSet != null).ToList();
			});
		}

		[Test]
		public void GetPositionOf100WaitlistedRequests()
		{
			Now.Is("2016-04-06 06:59");

			using (DataSource.OnThisThreadUse(tenantName))
				AsSystem.Logon(tenantName, businessUnitId);

			WithUnitOfWork.Do(() =>
			{
				foreach (var request in _requests.Take(100))
				{
					AbsenceRequestWaitlistProvider.GetPositionInWaitlist((IAbsenceRequest)request.Request);
				}

			});
		}
	}
}
