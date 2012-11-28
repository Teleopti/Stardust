using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
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

	public class ExistingApprovedAbsenceRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public AbsenceRequest AbsenceRequest;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var today = DateTime.UtcNow.Date;
			AbsenceRequest = new AbsenceRequest(TestData.Absence, new DateTimePeriod(today, today.AddHours(5)));
			PersonRequest = new PersonRequest(user, AbsenceRequest) { Subject = "I need some vacation" };
			PersonRequest.TrySetMessage("This is just a short text that doesn't say anything, except explaining that it doesn't say anything");
			PersonRequest.Pending();
			PersonRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());

			var requestRepository = new PersonRequestRepository(uow);

			requestRepository.Add(PersonRequest);
		}
	}

	public class ExistingDeniedAbsenceRequest : IUserDataSetup
	{
		private readonly string _denyReason;
		public PersonRequest PersonRequest;
		public AbsenceRequest AbsenceRequest;

		public ExistingDeniedAbsenceRequest()
		{
		}

		public ExistingDeniedAbsenceRequest(string denyReason)
		{
			_denyReason = denyReason;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var today = DateTime.UtcNow.Date;
			AbsenceRequest = new AbsenceRequest(TestData.Absence, new DateTimePeriod(today, today.AddHours(5)));
			PersonRequest = new PersonRequest(user, AbsenceRequest) { Subject = "I need some vacation" };
			PersonRequest.TrySetMessage("This is just a short text that doesn't say anything, except explaining that it doesn't say anything");
			PersonRequest.Pending();
			PersonRequest.Deny(null, _denyReason, new PersonRequestAuthorizationCheckerForTest());

			var requestRepository = new PersonRequestRepository(uow);

			requestRepository.Add(PersonRequest);
		}
	}

	public class ExistingShiftTradeRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest { get; set; }
		public ShiftTradeRequest ShiftTradeRequest { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var today = DateTime.UtcNow.Date;
			var tomorrow = today.AddDays(1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(user, user, new DateOnly(today), new DateOnly(tomorrow));
			ShiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			PersonRequest = new PersonRequest(user) {Subject = "I want to swap shift with myself"};
			PersonRequest.Request = ShiftTradeRequest;
			PersonRequest.TrySetMessage("This is a short text for the description of a shift trade request");

			var requestRepository = new PersonRequestRepository(uow);
			requestRepository.Add(PersonRequest);
		}

	}
}