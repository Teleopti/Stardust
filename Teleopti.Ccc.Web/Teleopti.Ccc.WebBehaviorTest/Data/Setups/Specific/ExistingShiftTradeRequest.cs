using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
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