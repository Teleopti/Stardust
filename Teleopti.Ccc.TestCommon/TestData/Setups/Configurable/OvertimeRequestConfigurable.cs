using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class OvertimeRequestConfigurable : IUserDataSetup
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Status { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var multiplicator = new MultiplicatorDefinitionSet("test multiplicator", MultiplicatorType.Overtime);
			var multiplicatorDefinitionSetRepository = new MultiplicatorDefinitionSetRepository(unitOfWork);
			multiplicatorDefinitionSetRepository.Add(multiplicator);

			var overtimeRequest = new OvertimeRequest(multiplicator,
				new DateTimePeriod(DateTime.SpecifyKind(StartTime, DateTimeKind.Utc),
					DateTime.SpecifyKind(EndTime, DateTimeKind.Utc)));
			var personRequest = new PersonRequest(person, overtimeRequest) { Subject = Subject };
			personRequest.TrySetMessage(Message);

			var requestRepository = new PersonRequestRepository(unitOfWork);

			if (Status == "AutoDenied")
			{
				personRequest.Deny(null, new PersonRequestAuthorizationCheckerForTest(), null, PersonRequestDenyOption.AutoDeny);
			}

			if (Status == "Pending")
			{
				personRequest.ForcePending();
			}

			if (Status == "Approved")
			{
				personRequest.ForcePending();
				personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest(), true);
			}

			requestRepository.Add(personRequest);
		}
	}
}