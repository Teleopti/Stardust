using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	[RequestPerformanceTuningTest]
	public class OvertimeRequestPerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public IUpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public FakeConfigReader ConfigReader;
		public IPersonRequestRepository PersonRequestRepository;
		public IStardustJobFeedback StardustJobFeedback;
		public IOvertimeRequestProcessor OvertimeRequestProcessor;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;

		public ISiteRepository SiteRepository;

		private IList<IPersonRequest> requests;
		private DateTime _nowDateTime;

		private const string tenantName = "Teleopti WFM";
		private readonly Guid businessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b");

		public override void OneTimeSetUp()
		{
			var schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(tenantName, schema);

			_nowDateTime = new DateTime(2016, 01, 16, 7, 0, 0).Utc();
			Now.Is(_nowDateTime);
			using (DataSource.OnThisThreadUse(tenantName))
				AsSystem.Logon(tenantName, businessUnitId);

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
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			WithUnitOfWork.Do(() =>
			{
				MultiplicatorDefinitionSetRepository.LoadAll();
				SiteRepository.LoadAllWithFetchingOpenHours();
				UpdateStaffingLevel.Update(period);
				requests = PersonRequestRepository.FindPersonRequestWithinPeriod(
					new DateTimePeriod(new DateTime(2016, 01, 16, 16, 0, 0).Utc(), new DateTime(2016, 01, 16, 20, 0, 0).Utc()));
			});
		}

		[Test]
		public void Run200Requests()
		{
			Now.Is("2016-01-16 07:01");

			using (DataSource.OnThisThreadUse(tenantName))
				AsSystem.Logon(tenantName, businessUnitId);
			Console.WriteLine($"Will process {requests.Count} requests");

			foreach (var request in requests)
			{
				WithUnitOfWork.Do(() =>
				{
					OvertimeRequestProcessor.Process(request);
				});
			}
			WithUnitOfWork.Do(() =>
			{
				var reqs = PersonRequestRepository.FindPersonRequestWithinPeriod(
					new DateTimePeriod(new DateTime(2016, 01, 16, 16, 0, 0).Utc(), new DateTime(2016, 01, 16, 20, 0, 0).Utc()));
				reqs.Count.Should().Be(200);
			});
		}

		[Test]
		public void Run200RequestsWithRequestPeriodSetting()
		{
			Now.Is("2016-01-16 07:01");

			using (DataSource.OnThisThreadUse(tenantName))
				AsSystem.Logon(tenantName, businessUnitId);
			Console.WriteLine($"Will process {requests.Count} requests");

			foreach (var request in requests)
			{
				WithUnitOfWork.Do(() =>
				{
					OvertimeRequestProcessor.Process(request);
				});
			}
			WithUnitOfWork.Do(() =>
			{
				var reqs = PersonRequestRepository.FindPersonRequestWithinPeriod(
					new DateTimePeriod(new DateTime(2016, 01, 16, 16, 0, 0).Utc(), new DateTime(2016, 01, 16, 20, 0, 0).Utc()));
				reqs.Count.Should().Be(200);
			});
		}
	}
}