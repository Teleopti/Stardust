using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	[System.ComponentModel.Category("ProcessBulkAbsenceRequest")]
	[DomainTest]
	public class ProcessBulkAbsenceRequestTest : ISetup
	{
		public IProcessMultipleAbsenceRequest Target;
		[Test,Ignore]
		public void ShouldProcessMultipleAbsenceRequests()
		{
			var absenceRequests = new List<NewAbsenceRequestCreatedEvent>() { new NewAbsenceRequestCreatedEvent()
			{
				PersonRequestId = new Guid("96f18da3-c29d-49dc-9763-a65c00d7e27e"),
				InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
				JobName = "Absence Request",
				LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
				LogOnDatasource = "Telia",
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				UserName = "KALA21 Lampinen, Kalle"
			} };
			Target.Process(absenceRequests);

		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new CommonModule(configuration));
			system.UseTestDouble<NewAbsenceRequestHandler>().For<NewAbsenceRequestHandler>();
			system.UseTestDouble<ProcessMultipleAbsenceRequest>().For<IProcessMultipleAbsenceRequest>();
		}
	}

	
	public class ProcessMultipleAbsenceRequest : IProcessMultipleAbsenceRequest
	{
		private readonly NewAbsenceRequestHandler _newAbsenceRequestHandler;

		public ProcessMultipleAbsenceRequest(NewAbsenceRequestHandler newAbsenceRequestHandler)
		{
			_newAbsenceRequestHandler = newAbsenceRequestHandler;
		}

		public void Process(List<NewAbsenceRequestCreatedEvent> absenceRequests)
		{
			foreach (var req in absenceRequests)
			{
				_newAbsenceRequestHandler.Handle(req);
			}
		}
	}

	public interface IProcessMultipleAbsenceRequest
	{
		void Process(List<NewAbsenceRequestCreatedEvent> absenceRequests);
	}
}
