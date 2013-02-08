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
		public ShiftTradeRequest ShiftTradeRequest { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public string Subject { get; set; }
		public DateOnly DateTo { get; set; }
		public DateOnly DateFrom { get; set; }

		public void Apply(IPerson user)
		{
			ScenarioUnitOfWorkState.UnitOfWorkAction(
				uow =>
				{
					var today = DateTime.UtcNow.Date;
					var tomorrow = today.AddDays(1);
					var sender = String.IsNullOrEmpty(From) ? user : GetOrCreatePerson(From, uow);
					var reciever = String.IsNullOrEmpty(To) ? user : GetOrCreatePerson(To, uow);

					var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, reciever, new DateOnly(today), new DateOnly(tomorrow));
					ShiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
					PersonRequest = new PersonRequest(reciever)
					{
						Subject = Subject == string.Empty ? "Swap shift with " + sender.Name : Subject
					};
					PersonRequest.Request = ShiftTradeRequest;
					PersonRequest.TrySetMessage("This is a short text for the description of a shift trade request");

					var requestRepository = new PersonRequestRepository(uow);
					requestRepository.Add(PersonRequest);
				});
		}

		private static IPerson GetOrCreatePerson(string name, IUnitOfWork uow)
		{
			var names = name.Split(' ');
			var personName = names.Length > 1 ? new Name(names[0], names[1]) : new Name(name, name);
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