using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PersonRequestFactory
	{
		public IPerson Person { get; set; }
		public Request Request { get; set; }

		public PersonRequestFactory()
		{
			Person = PersonFactory.CreatePerson();
			Request = new TextRequest(new DateTimePeriod());
		}

		public IPersonRequest CreateNewPersonRequest()
		{
			var request = new PersonRequest(Person, Request);
			return request;
		}

		public IPersonRequest CreatePersonRequest()
		{
			var request = CreateNewPersonRequest();
			request.Pending();
			return request;
		}

		public IPersonRequest CreatePersonRequest(IPerson person)
		{
			Person = person;

			var request = CreateNewPersonRequest();
			request.Pending();
			return request;
		}

		public IPersonRequest CreatePersonShiftTradeRequest(IPerson personTo, DateOnly requestDateOnly)
		{
			var shiftTradeSwapDetails = new IShiftTradeSwapDetail[]
			{
				new ShiftTradeSwapDetail(Person, personTo, requestDateOnly, requestDateOnly)
			};
			Person = personTo;
			Request = new ShiftTradeRequest(shiftTradeSwapDetails);

			var request = CreateNewPersonRequest();
			request.Pending();
			return request;
		}

		public IPersonRequest CreatePersonShiftTradeRequest(IPerson personFrom, IPerson personTo,
			DateOnly requestDateOnly)
		{
			var shiftTradeSwapDetails = new IShiftTradeSwapDetail[]
			{
				new ShiftTradeSwapDetail(personFrom, personTo, requestDateOnly, requestDateOnly)
			};
			Person = personTo;
			Request = new ShiftTradeRequest(shiftTradeSwapDetails);

			var request = CreateNewPersonRequest();
			request.Pending();
			return request;
		}

		public IAbsenceRequest CreateAbsenceRequest(IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			Request = new AbsenceRequest(absence, dateTimePeriod);
			return (IAbsenceRequest) CreatePersonRequest().Request;
		}

		public IAbsenceRequest CreateNewAbsenceRequest(IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			Request = new AbsenceRequest(absence, dateTimePeriod);
			return (IAbsenceRequest) CreateNewPersonRequest().Request;
		}

		public IPersonRequest CreateApprovedPersonRequest()
		{
			IPersonRequest personRequest = CreatePersonRequest();
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());

			return personRequest;
		}

		public static IPersonRequest CreateNewPersonRequest(IPerson person, IAbsence absence, DateTimePeriod period,
			string subject = "New Person Request")
		{
			IAbsenceRequest absenceRequest1 = new AbsenceRequest(absence, period);
			IPersonRequest personRequest = new PersonRequest(person, absenceRequest1)
			{
				Subject = subject
			};
			return personRequest;
		}

		public static IPersonRequest CreatePendingPersonRequest(IPerson person, IAbsence absence, DateTimePeriod period,
			string subject = "Pending Person Request")
		{
			IAbsenceRequest absenceRequest1 = new AbsenceRequest(absence, period);
			IPersonRequest personRequest = new PersonRequest(person, absenceRequest1)
			{
				Subject = subject
			};
			personRequest.Pending();
			return personRequest;
		}

		public static IPersonRequest CreateDeniedPersonRequest(IPerson person, IAbsence absence, DateTimePeriod period)
		{
			IAbsenceRequest absenceRequest1 = new AbsenceRequest(absence, period);
			IPersonRequest personRequest = new PersonRequest(person, absenceRequest1);
			personRequest.Pending();
			personRequest.Deny("-", new PersonRequestCheckAuthorization(), person,
				PersonRequestDenyOption.RequestExpired);
			return personRequest;
		}

		public static IPersonRequest CreateWaitlistedPersonRequest(IPerson person, IAbsence absence,
			DateTimePeriod period, string subject = "WaitList Person Request")
		{
			IAbsenceRequest absenceRequest1 = new AbsenceRequest(absence, period);
			IPersonRequest personRequest = new PersonRequest(person, absenceRequest1)
			{
				Subject = subject
			};
			personRequest.Pending();
			personRequest.Deny("-", new PersonRequestCheckAuthorization(), person, PersonRequestDenyOption.AutoDeny);
			return personRequest;

		}
	}
}
