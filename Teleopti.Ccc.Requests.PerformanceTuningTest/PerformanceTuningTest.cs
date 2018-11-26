using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	[RequestPerformanceTuningTest]
	public class PerformanceTuningTest
	{
		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public IPersonRequestRepository PersonRequestRepository;
		public MultiAbsenceRequestsHandler Target;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldHandle200AbsenceRequestsFast()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			var personRequests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				personRequests =
					PersonRequestRepository.FindPersonRequestWithinPeriod(
						new DateTimePeriod(new DateTime(2016, 2, 29, 23, 0, 0, DateTimeKind.Utc),
							new DateTime(2016, 3, 2, 23, 0, 0, DateTimeKind.Utc))).Where(x => x.Request is AbsenceRequest).ToList();

			});

			var absenceRequestIds = personRequests.Select(x => x.Id.GetValueOrDefault()).ToList();

			var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
			{
				PersonRequestIds = absenceRequestIds,
				InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
				LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
				LogOnDatasource = "Teleopti WFM",
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = DateTime.UtcNow
			};

			Target.Handle(newMultiAbsenceRequestsCreatedEvent);
		}

		[Test, Ignore("works only on olas small db")]
		public void ShouldHandle200AbsenceRequestsFastTwo()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("D71B7AF6-3DC9-490F-9A5D-A6CE00B2FAE5"));

			var personRequests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				personRequests =
					PersonRequestRepository.FindPersonRequestWithinPeriod(
							new DateTimePeriod(new DateTime(2016, 12, 05, 23, 0, 0, DateTimeKind.Utc),
								new DateTime(2016, 12, 8, 23, 0, 0, DateTimeKind.Utc)))
						.Where(x => x.Request is AbsenceRequest).ToList();
			});
			var absenceRequestIds = personRequests.Select(x => x.Id.GetValueOrDefault()).ToList();

			var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
			{
				PersonRequestIds = absenceRequestIds,
				InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
				LogOnBusinessUnitId = new Guid("D71B7AF6-3DC9-490F-9A5D-A6CE00B2FAE5"),
				LogOnDatasource = "Teleopti WFM",
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = DateTime.UtcNow
			};

			Target.Handle(newMultiAbsenceRequestsCreatedEvent);
		}
	}
}
