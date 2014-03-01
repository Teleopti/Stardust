using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class ExistingApprovedAbsenceRequestConfigurable : IUserDataSetup
	{
		public string Absence { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public ExistingApprovedAbsenceRequestConfigurable()
		{
			var today = DateTime.Now.ToUniversalTime();
			Absence = "Vocation";
			StartTime = today;
			EndTime = today.AddHours(5);
		}

		public ExistingApprovedAbsenceRequestConfigurable(ExistingApprovedAbsenceRequestConfig config)
		{
			Absence = config.Absence;
			StartTime = config.StartTime.ToUniversalTime();
			EndTime = config.EndTime.ToUniversalTime();
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var abs = new AbsenceRepository(uow).LoadAll().Single(x => x.Description.Name == Absence);
			var absenceRequest = new AbsenceRequest(abs, new DateTimePeriod(StartTime, EndTime));
			var personRequest = new PersonRequest(user, absenceRequest) { Subject = "I need some vacation" };
			personRequest.TrySetMessage("This is just a short text that doesn't say anything, except explaining that it doesn't say anything");
			personRequest.Pending();
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());

			var requestRepository = new PersonRequestRepository(uow);

			requestRepository.Add(personRequest);
		}
	}

	public class ExistingApprovedAbsenceRequestConfig
	{
		public string Absence { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}