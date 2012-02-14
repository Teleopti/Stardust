using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ExistingTextRequestOver2Days : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public TextRequest TextRequest;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var startTime = DateTime.UtcNow;
			if (DateHelper.GetLastDateInWeek(startTime.Date, cultureInfo) == startTime.Date)
				startTime = startTime.AddDays(-1);
			var endTime = startTime.AddDays(1);
			var period = new DateTimePeriod(startTime, endTime);

			TextRequest = new TextRequest(period);
			PersonRequest = new PersonRequest(user, TextRequest) {Subject = "I need cakes for 2 days"};
			PersonRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");

			var requestRepository = new PersonRequestRepository(uow);

			requestRepository.Add(PersonRequest);
		}
	}
}