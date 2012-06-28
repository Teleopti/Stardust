using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ExistingAbsenceRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public AbsenceRequest AbsenceRequest;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var today = DateTime.UtcNow.Date;
			AbsenceRequest = new AbsenceRequest(TestData.Absence, new DateTimePeriod(today, today.AddHours(5)));
			PersonRequest = new PersonRequest(user, AbsenceRequest) {Subject = "I need some vacation"};
			PersonRequest.TrySetMessage("This is just a short text that doesn't say anything, except explaining that it doesn't say anything");

			var requestRepository = new PersonRequestRepository(uow);

			requestRepository.Add(PersonRequest);
		}
	}
}