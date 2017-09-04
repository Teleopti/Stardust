using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
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
		private ICurrentUnitOfWork _currentUnitOfWork;

		[SetUp]
		public void Setup()
		{

			_scenario = new FakeCurrentScenario();
			_currentUnitOfWork = CurrentUnitOfWork.Make();

			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository,
				new SchedulingResultStateHolder());
			_scheduleStorage = new FakeScheduleStorage();
			var scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleStorage,
				new FromFactory(() => new FakeCurrentUnitOfWorkFactory().Current()), new EmptyScheduleDayDifferenceSaver());
			_saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver,personAbsenceAccountRepository,
				new DoNothingScheduleDayChangeCallBack());

			_personRepository = new FakePersonRepositoryLegacy2();

			_personAbsenceRepository = new FakePersonAbsenceRepositoryLegacy();
			_personAbsenceCreator = new PersonAbsenceCreator(_saveSchedulePartService,_businessRulesForAccountUpdate);


			_person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			_person.WithId();
			_personRepository.Has(_person);

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(_person);
			_personAbsenceRemover = new PersonAbsenceRemover(_businessRulesForAccountUpdate,_saveSchedulePartService,
				_personAbsenceCreator,loggedOnUser
				,new CheckingPersonalAccountDaysProvider(new FakePersonAbsenceAccountRepository()));


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
				_person,
				mainActivity, new DateTimePeriod(2016,10,1,8,2016,10,1,17));


			_scheduleStorage.Add(selectedPersonAbsence);
			_scheduleStorage.Add(notSelectedPersonAbsence);
			_scheduleStorage.Add(personAssignment);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				PersonId = _person.Id.Value,
				PersonAbsenceId = selectedPersonAbsence.Id.Value,
				Date = new DateOnly(2016,10,1)
			};

			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario,_personAbsenceRepository,_scheduleStorage,
				_personRepository,_personAbsenceRemover, _currentUnitOfWork);

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
				PersonAbsenceId = selectedPersonAbsence.Id.Value,
				Date = new DateOnly(2016,10,1)
			};


			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario,_personAbsenceRepository,_scheduleStorage,
				_personRepository,_personAbsenceRemover, _currentUnitOfWork);

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
				_person,
				mainActivity, new DateTimePeriod(2016,10,2,22,2016,10,3,6));


			_scheduleStorage.Add(personAbsence);
			_scheduleStorage.Add(personAssignment);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				PersonId = _person.Id.Value,
				PersonAbsenceId = personAbsence.Id.Value,
				Date = new DateOnly(2016,10,2)
			};

			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario,_personAbsenceRepository,_scheduleStorage,
				_personRepository,_personAbsenceRemover, _currentUnitOfWork);

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
				_person,
				mainActivity, new DateTimePeriod(2016,10,1,8,2016,10,1,17));


			_scheduleStorage.Add(selectedPersonAbsence);
			_scheduleStorage.Add(selectedPersonAbsence2);
			_scheduleStorage.Add(personAssignment);


			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario,_personAbsenceRepository,_scheduleStorage,
				_personRepository,_personAbsenceRemover, _currentUnitOfWork);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				PersonId = _person.Id.Value,
				PersonAbsenceId = selectedPersonAbsence.Id.Value,
				Date = new DateOnly(2016, 10, 1)
			};

			target.Handle(command);

			command.PersonAbsenceId = selectedPersonAbsence2.Id.Value;
			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var allPersonAbsences = _scheduleStorage.LoadAll().Where(s => s is PersonAbsence).ToList();

			allPersonAbsences.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveByDayWithTimezoneAwarenessWhenRemovingOnMultidayAbsence()
		{
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
			var periodForAbsence = new DateTimePeriod(2017, 2, 20, 0, 2017, 2, 23, 8);

			var absenceLayer = new AbsenceLayer(new Absence(), periodForAbsence);
			var personAbsence = new PersonAbsence(_person, _scenario.Current(), absenceLayer);
			personAbsence.WithId();
			_personAbsenceRepository.Has(personAbsence);

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				_person,
				_scenario.Current(), new DateTimePeriod(2017, 2, 21, 0, 2017, 2, 21, 8));

			_scheduleStorage.Add(personAbsence);
			_scheduleStorage.Add(personAssignment);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				PersonId = _person.Id.Value,
				PersonAbsenceId = personAbsence.Id.Value,
				Date = new DateOnly(2017, 2, 21)
			};

			var target = new RemoveSelectedPersonAbsenceCommandHandler(_scenario, _personAbsenceRepository, _scheduleStorage,
				_personRepository, _personAbsenceRemover, _currentUnitOfWork);

			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);


			var allPersonAbsences = _scheduleStorage.LoadAll().Where(s => s is PersonAbsence).ToList();
			allPersonAbsences.Count.Should().Be.EqualTo(2);

			allPersonAbsences.First().Period.Should().Be.EqualTo(new DateTimePeriod(2017, 2, 20, 0, 2017, 2, 20, 15));
			allPersonAbsences.Last().Period.Should().Be.EqualTo(new DateTimePeriod(2017, 2, 21, 15, 2017, 2, 23, 8));
		}
	}
}
