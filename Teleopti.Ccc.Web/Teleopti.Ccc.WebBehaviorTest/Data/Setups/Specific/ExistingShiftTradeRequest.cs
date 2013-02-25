using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class ExistingShiftTradeRequest : IPostSetup
	{
		public PersonRequest PersonRequest { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
		public DateTime? DateTo { get; set; }
		public DateTime? DateFrom { get; set; }
		public bool Pending { get; set; }

		public void Apply(IPerson user, IUnitOfWork uow)
		{
			var dateTimefrom = DateFrom ?? DateTime.UtcNow.Date;
			var dateTimeTo = DateTo ?? dateTimefrom.AddDays(1);
			var sender = String.IsNullOrEmpty(From) ? user : GetOrCreatePerson(From, uow);
			var reciever = String.IsNullOrEmpty(To) ? user : GetOrCreatePerson(To, uow);
			var message = String.IsNullOrEmpty(Message)
				              ? "This is a short text for the description of a shift trade request"
				              : Message; 

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, reciever, new DateOnly(dateTimefrom), new DateOnly(dateTimeTo));
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			
			PersonRequest = new PersonRequest(reciever)
			{
				Subject = Subject == string.Empty ? "Swap shift with " + sender.Name : Subject
			};
			if(Pending)PersonRequest.Pending();
			PersonRequest.TrySetMessage(message);
			PersonRequest.Request = shiftTradeRequest;

			var setShiftTraderequestCheckSum = new ShiftTradeRequestSetChecksum(new ScenarioRepository(uow),
																									  new ScheduleRepository(uow));

			setShiftTraderequestCheckSum.SetChecksum(shiftTradeRequest);
			var requestRepository = new PersonRequestRepository(uow);
			requestRepository.Add(PersonRequest);
		}

		private static IPerson GetOrCreatePerson(string name, IUnitOfWork uow)
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