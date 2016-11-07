﻿using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class RemoveSelectedPersonAbsenceComandHandlerTest
	{
		private SaveSchedulePartService _saveSchedulePartService;
		private FakeScheduleStorage _scheduleStorage;
		private BusinessRulesForPersonalAccountUpdate _businessRulesForAccountUpdate;
		private FakeCurrentScenario _scenario;
		private FakePersonRepository _personRepository;
		private FakePersonAbsenceRepository _personAbsenceRepository;
		private PersonAbsenceCreator _personAbsenceCreator;
		private PersonAbsenceRemover _personAbsenceRemover;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{

			_scenario = new FakeCurrentScenario();

			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository,
				new SchedulingResultStateHolder());
			_scheduleStorage = new FakeScheduleStorage();
			var scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleStorage,
				new FromFactory(() => new FakeCurrentUnitOfWorkFactory().Current()));
			_saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver,personAbsenceAccountRepository,
				new DoNothingScheduleDayChangeCallBack());

			_personRepository = new FakePersonRepository();

			_personAbsenceRepository = new FakePersonAbsenceRepository();
			_personAbsenceCreator = new PersonAbsenceCreator(_saveSchedulePartService,_businessRulesForAccountUpdate);


			_person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			_person.WithId();
			_personRepository.Has(_person);

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(_person);
			_personAbsenceRemover = new PersonAbsenceRemover(_businessRulesForAccountUpdate,_saveSchedulePartService,
				_personAbsenceCreator,loggedOnUser,
				new AbsenceRequestCancelService(new PersonRequestAuthorizationCheckerForTest(),_scenario)
				,new CheckingPersonalAccountDaysProvider(new FakePersonAbsenceAccountRepository()),
				new PersonRequestAuthorizationCheckerForTest());


		}

		[Test]
		public void ShouldRemoveSelectedPersonAbsenceFromShiftDay()
		{

			var periodForSelectedAbsence = new DateTimePeriod(2016,10,1,10,2016,10,1,11);
			var periodForNotSelectedAbsence = new DateTimePeriod(2016,10,1,14,2016,10,1,15);


			var selectedPersonAbsence = new PersonAbsence(_person,_scenario.Current(),new AbsenceLayer(new Absence(),periodForSelectedAbsence));
			var notSelectedPersonAbsence = new PersonAbsence(_person,_scenario.Current(),new AbsenceLayer(new Absence(),periodForNotSelectedAbsence));
			selectedPersonAbsence.WithId();
			notSelectedPersonAbsence.WithId();
			_personAbsenceRepository.Has(selectedPersonAbsence);
			_personAbsenceRepository.Has(notSelectedPersonAbsence);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,_person,
				new DateTimePeriod(2016,10,1,8,2016,10,1,17));


			_scheduleStorage.Add(selectedPersonAbsence);
			_scheduleStorage.Add(notSelectedPersonAbsence);
			_scheduleStorage.Add(personAssignment);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				PersonId = _person.Id.Value,
				PersonAbsenceIds = new[] { selectedPersonAbsence.Id.Value },
				Date = new DateOnly(2016,10,1),
			};

			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario,_personAbsenceRepository,_scheduleStorage,
				_personRepository,_personAbsenceRemover);

			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);


			var allPersonAbsences = _scheduleStorage.LoadAll().Where(s => s is PersonAbsence).ToList();

			allPersonAbsences.Single().Period.Should().Be.EqualTo(new DateTimePeriod(2016,10,1,14,2016,10,1,15));

		}


		[Test]
		public void ShouldRemoveSelectedPersonAbsenceFromEmptyDay()
		{
			var periodForSelectedAbsence = new DateTimePeriod(2016,10,1,8,2016,10,1,17);

			var selectedPersonAbsence = new PersonAbsence(_person,_scenario.Current(),new AbsenceLayer(new Absence(),periodForSelectedAbsence));

			selectedPersonAbsence.WithId();

			_personAbsenceRepository.Has(selectedPersonAbsence);
			_scheduleStorage.Add(selectedPersonAbsence);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				PersonId = _person.Id.Value,
				PersonAbsenceIds = new[] { selectedPersonAbsence.Id.Value },
				Date = new DateOnly(2016,10,1),
			};


			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario,_personAbsenceRepository,_scheduleStorage,
				_personRepository,_personAbsenceRemover);

			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);


			var allPersonAbsences = _scheduleStorage.LoadAll().Where(s => s is PersonAbsence).ToList();

			allPersonAbsences.Count.Should().Be.EqualTo(0);

		}

		[Test]
		public void ShouldRemoveAbsenceThatCoversOvernightShift()
		{

			var periodForAbsence = new DateTimePeriod(2016,10,1,0,2016,10,4,0);

			var absenceLayer = new AbsenceLayer(new Absence(),periodForAbsence);
			var personAbsence = new PersonAbsence(_person,_scenario.Current(),absenceLayer);
			personAbsence.WithId();
			_personAbsenceRepository.Has(personAbsence);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,_person,
				new DateTimePeriod(2016,10,2,22,2016,10,3,6));


			_scheduleStorage.Add(personAbsence);
			_scheduleStorage.Add(personAssignment);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				PersonId = _person.Id.Value,
				PersonAbsenceIds = new[] { personAbsence.Id.Value },
				Date = new DateOnly(2016,10,2),
			};

			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario,_personAbsenceRepository,_scheduleStorage,
				_personRepository,_personAbsenceRemover);

			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);


			var allPersonAbsences = _scheduleStorage.LoadAll().Where(s => s is PersonAbsence).ToList();
			allPersonAbsences.Count.Should().Be.EqualTo(2);

			allPersonAbsences.First().Period.Should().Be.EqualTo(new DateTimePeriod(2016,10,1,0,2016,10,2,0));
			allPersonAbsences.Last().Period.Should().Be.EqualTo(new DateTimePeriod(2016,10,3,0,2016,10,4,0));
		}

		[Test]
		public void ShouldRemoveMultipleAbsencesWithinAShiftDay()
		{
			var periodForAbsence1 = new DateTimePeriod(2016,10,1,10,2016,10,1,11);
			var periodForAbsence2 = new DateTimePeriod(2016,10,1,14,2016,10,1,15);

			var selectedPersonAbsence = new PersonAbsence(_person,_scenario.Current(),new AbsenceLayer(new Absence(),periodForAbsence1));
			var selectedPersonAbsence2 = new PersonAbsence(_person,_scenario.Current(),new AbsenceLayer(new Absence(),periodForAbsence2));
			selectedPersonAbsence.WithId();
			selectedPersonAbsence2.WithId();
			_personAbsenceRepository.Has(selectedPersonAbsence);
			_personAbsenceRepository.Has(selectedPersonAbsence2);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,_person,
				new DateTimePeriod(2016,10,1,8,2016,10,1,17));


			_scheduleStorage.Add(selectedPersonAbsence);
			_scheduleStorage.Add(selectedPersonAbsence2);
			_scheduleStorage.Add(personAssignment);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				PersonId = _person.Id.Value,
				PersonAbsenceIds = new[] { selectedPersonAbsence.Id.Value,selectedPersonAbsence2.Id.Value },
				Date = new DateOnly(2016,10,1),
			};

			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario,_personAbsenceRepository,_scheduleStorage,
				_personRepository,_personAbsenceRemover);

			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var allPersonAbsences = _scheduleStorage.LoadAll().Where(s => s is PersonAbsence).ToList();

			allPersonAbsences.Count().Should().Be.EqualTo(0);
		}
	}
}
