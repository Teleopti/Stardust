using System;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class TwoExistingTextRequestChangedOnDifferentTimes : IPostSetup
	{
		public PersonRequest PersonRequest1;
		public PersonRequest PersonRequest2;

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{

			TestDataSetup.UnitOfWorkAction(
				uow =>
					{
						var textRequest1 = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5)));
						PersonRequest1 = new PersonRequest(user, textRequest1) {Subject = "I need some cake"};
						PersonRequest1.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");
						var requestRepository = new PersonRequestRepository(uow);
						requestRepository.Add(PersonRequest1);
					});

			Thread.Sleep(1010);

			TestDataSetup.UnitOfWorkAction(
				uow =>
					{
						var textRequest2 = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5)));
						PersonRequest2 = new PersonRequest(user, textRequest2) {Subject = "I need some cake"};
						PersonRequest2.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");
						var requestRepository = new PersonRequestRepository(uow);
						requestRepository.Add(PersonRequest2);
					});

		}
	}
}