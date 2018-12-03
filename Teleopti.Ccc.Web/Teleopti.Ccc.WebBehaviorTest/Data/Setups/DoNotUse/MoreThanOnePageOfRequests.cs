using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class MoreThanOnePageOfRequests : IUserDataSetup
	{
		public int PageSize = 20;
		public int RequestCount = 30;

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var requestRepository = new PersonRequestRepository(unitOfWork);

			Enumerable.Range(0, RequestCount)
				.ForEach(i =>
								{
									var time = DateTime.UtcNow.AddMinutes(15 * i);
									var request = new TextRequest(new DateTimePeriod(time, time.AddHours(1)));
									var personRequest = new PersonRequest(person, request) { Subject = string.Format("I need {0} cake(s)", i) };
									personRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");
									requestRepository.Add(personRequest);
								});

		}
	}
}