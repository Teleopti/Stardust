using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;

namespace Teleopti.Ccc.AbsenceRequest.PerformanceTest
{
	[RequestPerformanceTuningTest]
	[AllTogglesOn]
	public class MultiAbsenceRequestPerformanceTuningTest
	{
		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public IPersonRequestRepository PersonRequestRepository;
		public MultiAbsenceRequestsHandlerRobustToggleOn Target;
		public WithUnitOfWork WithUnitOfWork;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public FakeConfigReader ConfigReader;

		//[Test,Ignore("Waiting for a fast lane Build")]
		//[ToggleOff(Toggles.WFM_AbsenceRequest_Robust_Processing_79988)]
		[Test]
		public void ShouldHandle200AbsenceRequestsFast()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "Prepare200Request.sql";
				var script = File.ReadAllText(path);

				using (var command = new SqlCommand(script, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			var personRequests = new List<IPersonRequest>();
			var ids = new List<Guid>();
			WithUnitOfWork.Do(() =>
			{
				personRequests =
					PersonRequestRepository.FindPersonRequestWithinPeriod(
						new DateTimePeriod(new DateTime(2016, 2, 29, 23, 0, 0, DateTimeKind.Utc),
							new DateTime(2016, 3, 2, 23, 0, 0, DateTimeKind.Utc))).Where(x => x.Request is Domain.AgentInfo.Requests.AbsenceRequest).ToList();

				foreach (var personRequest in personRequests)
				{
					var queuedAbsenceRequest = new QueuedAbsenceRequest
					{
						PersonRequest = personRequest.Id.GetValueOrDefault(),
						Created = personRequest.CreatedOn.GetValueOrDefault(),
						StartDateTime = personRequest.Request.Period.StartDateTime,
						EndDateTime = personRequest.Request.Period.EndDateTime
					};
					QueuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
					ids.Add(queuedAbsenceRequest.Id.GetValueOrDefault());
					//personRequest.Pending();
				}


			});

			var absenceRequestIds = personRequests.Select(x => x.Id.GetValueOrDefault()).ToList();


			var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
			{
				PersonRequestIds = absenceRequestIds,
				InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
				LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
				LogOnDatasource = "Teleopti WFM",
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = DateTime.UtcNow,
				Ids = ids
			};

			Target.Handle(newMultiAbsenceRequestsCreatedEvent);
		}
	}
}
