using System;
using System.Threading;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class TwoExistingTextRequestChangedOnDifferentTimes : IDelayedSetup
	{
		public PersonRequest PersonRequest1;
		public PersonRequest PersonRequest2;

		public void Apply(IPerson user, ICurrentUnitOfWork currentUnitOfWork)
		{
			var textRequest1 = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5)));
			PersonRequest1 = new PersonRequest(user, textRequest1) {Subject = "I need some cake"};
			PersonRequest1.TrySetMessage(
				"This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");
			var requestRepository1 = PersonRequestRepository.DONT_USE_CTOR(currentUnitOfWork);
			requestRepository1.Add(PersonRequest1);
					
			Thread.Sleep(1010);

			var textRequest2 = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5)));
			PersonRequest2 = new PersonRequest(user, textRequest2) {Subject = "I need some cake"};
			PersonRequest2.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");
			var requestRepository2 = PersonRequestRepository.DONT_USE_CTOR(currentUnitOfWork);
			requestRepository2.Add(PersonRequest2);

		}
	}
}