using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.WebBehaviorTest.Bindings.DoNotUse;


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
		public bool Accepted { get; set; }

		public void Apply(IPerson user, ICurrentUnitOfWork uow)
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
			var currentAuthorization = new FullPermission();
			var personAssignmentRepository = PersonAssignmentRepository.DONT_USE_CTOR(uow);
			var personAbsenceRepository = new PersonAbsenceRepository(uow);
			var agentDayScheduleTagRepository = AgentDayScheduleTagRepository.DONT_USE_CTOR(uow);
			var noteRepository = new NoteRepository(uow);
			var publicNoteRepository = new PublicNoteRepository(uow);
			var preferenceDayRepository = new PreferenceDayRepository(uow);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(uow);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(uow);
			var setShiftTraderequestCheckSum = new ShiftTradeRequestSetChecksum(
				new DefaultScenarioFromRepository(ScenarioRepository.DONT_USE_CTOR(uow)),
				new ScheduleStorage(uow, personAssignmentRepository, personAbsenceRepository,
					new MeetingRepository(uow), agentDayScheduleTagRepository, noteRepository,
					publicNoteRepository, preferenceDayRepository,
					studentAvailabilityDayRepository, PersonAvailabilityRepository.DONT_USE_CTOR(uow),
					new PersonRotationRepository(uow), overtimeAvailabilityRepository,
					new PersistableScheduleDataPermissionChecker(currentAuthorization),
					new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
						() => personAbsenceRepository,
						() => preferenceDayRepository, () => noteRepository,
						() => publicNoteRepository,
						() => studentAvailabilityDayRepository,
						() => agentDayScheduleTagRepository,
						() => overtimeAvailabilityRepository), currentAuthorization));

			setShiftTraderequestCheckSum.SetChecksum(shiftTradeRequest); 
			var requestRepository = new PersonRequestRepository(uow);
			if (Approved)
			{
				PersonRequest.ForcePending();
				PersonRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
			}

			if (AutoDenied)
			{
				PersonRequest.Deny("denyReason", new PersonRequestAuthorizationCheckerForTest(), sender);
			}
			if (HasBeenReferred)
			{
				shiftTradeRequest.Refer(new PersonRequestAuthorizationCheckerForTest());
			}
			if (Accepted)
			{
				PersonRequest.ForcePending();
				shiftTradeRequest.Accept(reciever, new EmptyShiftTradeRequestSetChecksum(), new PersonRequestAuthorizationCheckerConfigurable());
			}
			requestRepository.Add(PersonRequest);

		}

		private static IPerson getOrCreatePerson(string name, ICurrentUnitOfWork uow)
		{
			var personName = new Name(name, "");
			if (name.Contains(" "))
			{
				var splitted = name.Split(' ');
				personName = new Name(splitted[0], splitted[1]);
			}
			var personRepository = new PersonRepository(uow, null, null);
			var people = personRepository.LoadAll();
			var person = people.FirstOrDefault(p => p.Name == personName);
			if (person == null)
			{
				var partTimePercentageRepository = PartTimePercentageRepository.DONT_USE_CTOR(uow);
				var contractRepository = ContractRepository.DONT_USE_CTOR(uow, null, null);
				var contractScheduleRepository = ContractScheduleRepository.DONT_USE_CTOR(uow);
				person = PersonFactory.CreatePerson(personName);
				
				var contract = PersonContractFactory.CreatePersonContract();
				partTimePercentageRepository.Add(contract.PartTimePercentage);
				contractRepository.Add(contract.Contract);
				contractScheduleRepository.Add(contract.ContractSchedule);
				person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 01, 01),
					contract,
					DefaultTeam.Get()));
				personRepository.Add(person);
			}
			return person;
		}
	}
}