using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class TextRequestConfigurable : IUserDataSetup
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			StartTime = DateTime.SpecifyKind(StartTime, DateTimeKind.Utc);
			EndTime = DateTime.SpecifyKind(EndTime, DateTimeKind.Utc);
			var textRequest = new TextRequest(new DateTimePeriod(StartTime, EndTime));
			var personRequest = new PersonRequest(user, textRequest) { Subject = "I need some cake" };
			personRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");

			var requestRepository = new PersonRequestRepository(currentUnitOfWork);

			requestRepository.Add(personRequest);
		}
	}
}