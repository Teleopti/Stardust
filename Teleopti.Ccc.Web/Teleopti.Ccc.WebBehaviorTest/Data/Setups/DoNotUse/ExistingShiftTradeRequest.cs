using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ExistingShiftTradeRequest : IDelayedSetup
	{
		public PersonRequest PersonRequest { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
		public DateTime? DateTo { get; set; }
		public DateTime? DateFrom { get; set; }
		public bool Pending { get; set; }
		public bool Approved { get; set; }
		public bool AutoDenied { get; set; }
		public bool HasBeenReferred { get; set; }

		public void Apply(IPerson user, IUnitOfWork iDontUse)
		{
			using (var uow = GlobalUnitOfWorkState.CurrentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{

				var dateTimefrom = DateFrom ?? DateTime.UtcNow.Date;
				var dateTimeTo = DateTo ?? dateTimefrom.AddDays(1);
				var sender = String.IsNullOrEmpty(From) ? user : getOrCreatePerson(From, uow);
				var reciever = String.IsNullOrEmpty(To) ? user : getOrCreatePerson(To, uow);
				var message = String.IsNullOrEmpty(Message)
								  ? "This is a short text for the description of a shift trade request"
								  : Message;

				var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, reciever, new DateOnly(dateTimefrom), new DateOnly(dateTimeTo));
				var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });

				PersonRequest = new PersonRequest(sender)
				{
					Subject = Subject == string.Empty ? "Swap shift with " + sender.Name : Subject
				};
				if (Pending)
				{
					PersonRequest.Pending();
				}

				PersonRequest.TrySetMessage(message);
				PersonRequest.Request = shiftTradeRequest;
				var setShiftTraderequestCheckSum = new ShiftTradeRequestSetChecksum(new DefaultScenarioFromRepository(new ScenarioRepository(uow)), new ScheduleRepository(uow));

				setShiftTraderequestCheckSum.SetChecksum(shiftTradeRequest);
				var requestRepository = new PersonRequestRepository(uow);
				if (Approved)
				{
					PersonRequest.ForcePending();
					PersonRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
				}

				if (AutoDenied)
				{
					PersonRequest.Deny(sender, "denyReason", new PersonRequestAuthorizationCheckerForTest());
				}
				if (HasBeenReferred)
				{
					shiftTradeRequest.Refer(new PersonRequestAuthorizationCheckerForTest());
				}
				requestRepository.Add(PersonRequest);


				uow.PersistAll();
			}

		}

		private static IPerson getOrCreatePerson(string name, IUnitOfWork uow)
		{
			var personName = new CreateName().FromString(name);
			var personRepository = new PersonRepository(uow);
			var people = personRepository.LoadAll();
			var person = people.FirstOrDefault(p => p.Name == personName);
			if (person == null)
			{
				person = PersonFactory.CreatePerson(personName);
				personRepository.Add(person);
			}
			return person;
		}
	}
}