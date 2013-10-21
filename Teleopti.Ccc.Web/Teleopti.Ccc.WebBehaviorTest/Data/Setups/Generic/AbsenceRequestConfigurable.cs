using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class AbsenceRequestConfigurable : IUserDataSetup
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var absenceRequest = new AbsenceRequest(TestData.Absence, new DateTimePeriod(DateTime.SpecifyKind(StartTime, DateTimeKind.Utc), DateTime.SpecifyKind(EndTime, DateTimeKind.Utc)));
			var personRequest = new PersonRequest(user, absenceRequest) { Subject = "I need some vacation" };
			personRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");

			var requestRepository = new PersonRequestRepository(uow);

			requestRepository.Add(personRequest);
		}
	}
}