using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class TextRequestConfigurable : IUserDataSetup
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Status { get; set; }
		public string Subject { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			StartTime = DateTime.SpecifyKind(StartTime, DateTimeKind.Utc);
			EndTime = DateTime.SpecifyKind(EndTime, DateTimeKind.Utc);
			var textRequest = new TextRequest(new DateTimePeriod(StartTime, EndTime));
			var personRequest = new PersonRequest(person, textRequest) { Subject = string.IsNullOrEmpty(Subject) ? "I need some cake" : Subject };
			personRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");

			if (Status == "Pending")
			{
				personRequest.ForcePending();
			}

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(personRequest);
		}
	}
}