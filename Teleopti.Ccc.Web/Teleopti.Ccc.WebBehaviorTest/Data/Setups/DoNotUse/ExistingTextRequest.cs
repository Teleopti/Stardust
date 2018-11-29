using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ExistingTextRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public TextRequest TextRequest;

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var today = DateTime.UtcNow.Date;
			TextRequest = new TextRequest(new DateTimePeriod(today, today.AddHours(5)));
			PersonRequest = new PersonRequest(person, TextRequest) {Subject = "I need some cake"};
			PersonRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}

	public class ExistingTextRequestCreatedOnTestDate : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public TextRequest TextRequest;

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{

			var today = CurrentTime.Value().ToUniversalTime();
			TextRequest = new TextRequest(new DateTimePeriod(today, today.AddHours(5)));
			PersonRequest = new PersonRequest(person, TextRequest) {Subject = "I need some cake"};
			PersonRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}
	 

	public class ExistingPendingTextRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public TextRequest TextRequest;

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			TextRequest = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5)));
			PersonRequest = new PersonRequest(person, TextRequest) { Subject = "I need some cake" };
			PersonRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");
			PersonRequest.Pending();

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}

	public class ExistingApprovedTextRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public TextRequest TextRequest;

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			TextRequest = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5)));
			PersonRequest = new PersonRequest(person, TextRequest) { Subject = "I need some cake" };
			PersonRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");
			PersonRequest.Pending();
			PersonRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}
	public class ExistingDeniedTextRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public TextRequest TextRequest;

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			TextRequest = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5)));
			PersonRequest = new PersonRequest(person, TextRequest) { Subject = "I need some cake" };
			PersonRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");
			PersonRequest.Pending();
			PersonRequest.Deny( null, new PersonRequestAuthorizationCheckerForTest());

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}
}