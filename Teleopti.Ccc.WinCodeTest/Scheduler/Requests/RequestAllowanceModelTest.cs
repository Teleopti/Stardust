using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class RequestAllowanceModelTest
    {
        private IRequestAllowanceModel _target;
        private IBudgetGroup _budgetGroup;
        private IUnitOfWorkFactory _uowFactory;
        private IBudgetDayRepository _budgetDayRepository;
        private IBudgetGroupRepository _budgetGroupRepository;
        private ICurrentScenario _scenarioRepository;
        private IScheduleProjectionReadOnlyPersister _scheduleProjPersister;
        
        [SetUp]
        public void Setup()
        {
            _budgetGroup = new BudgetGroup();
            _uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            _budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();
            _budgetGroupRepository = MockRepository.GenerateMock<IBudgetGroupRepository>();
			_scenarioRepository = MockRepository.GenerateMock<ICurrentScenario>();
            _scheduleProjPersister = MockRepository.GenerateMock<IScheduleProjectionReadOnlyPersister>();

            _target = new RequestAllowanceModel(_uowFactory, _budgetDayRepository, _budgetGroupRepository, _scenarioRepository, _scheduleProjPersister );
        }

        [Test]
        public void ShouldInitializeRequestAllowanceModel()
        {
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { _budgetGroup});
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());

            _target.Initialize(_budgetGroup, new DateOnly(2000, 1, 1));

            _target.BudgetGroups.Count().Should().Be.EqualTo(1);
        }
        
        [Test]
        public void ShouldReturnDateOnly()
        {
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { _budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());
            
            _target.Initialize(_budgetGroup, new DateOnly(2000, 1, 1));
            _target.SelectedDate.Should().Be.EqualTo(new DateOnly(2000, 1, 1));
        }

        [Test]
        public void ShouldInitializeSelectedBudgetGroup()
        {
            var budgetGroup = new BudgetGroup();
            budgetGroup.Name = "Test Budget Group";
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());
           
            _target.Initialize(budgetGroup, new DateOnly(2000, 1, 1));

            _target.SelectedBudgetGroup.Name.Should().Be.EqualTo("Test Budget Group");
        }

        [Test]
        public void ShouldReloadModel()
        {
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { _budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());
            
            _target.Initialize(_budgetGroup, new DateOnly(2011, 12, 1));
            
            _target.ReloadModel(new DateOnlyPeriod(new DateOnly(2011,12,1), new DateOnly(2011,12,6)), true);
            _target.BudgetGroups.Count.Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldMoveToPreviousWeek()
        {
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { _budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());

            _target.Initialize(_budgetGroup, new DateOnly(2011, 12, 19));
            _target.MoveToPreviousWeek();
            _target.VisibleWeek.Should().Be.EqualTo(new DateOnlyPeriod(new DateOnly(2011,12,12), new DateOnly(2011,12,18)));

        }

        [Test]
        public void ShouldMoveToNextWeek()
        {
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { _budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());

            _target.Initialize(_budgetGroup, new DateOnly(2011, 12, 19));
            _target.MoveToNextWeek();
            _target.VisibleWeek.Should().Be.EqualTo(new DateOnlyPeriod(new DateOnly(2011, 12, 26), new DateOnly(2012, 1, 1)));

        }

       [Test, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ShouldAddPersonPeriod()
        {
            var budgetGroup = new BudgetGroup();
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());
            var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
                                                  new PersonContract(new Contract("Permanent"),
                                                                     new PartTimePercentage("50"),
                                                                     new ContractSchedule("Test")), new Team());

            personPeriod.BudgetGroup = budgetGroup;
             _uowFactory.Stub(x => x.CurrentUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());

            _target.Initialize(budgetGroup, new DateOnly(2000, 1, 1));
        }

        [Test]
        public void ShouldLoadAbsencesInBudgetGroup()
        {
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { _budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());
            
            _target.Initialize(_budgetGroup, new DateOnly(2011, 12, 19));
            _target.AbsencesInBudgetGroup.Count.Should().Be.EqualTo(0);
        }

        [Test, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ShouldHaveCustomShrinkages()
        {
            var budgetGroup = new BudgetGroup();
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());

            var personPeriod = new PersonPeriod(new DateOnly(2011, 12, 19),
                                                new PersonContract(new Contract("Permanent"),
                                                                     new PartTimePercentage("50"),
                                                                     new ContractSchedule("Test")),
                                                new Team());

            var customShrinkage = new CustomShrinkage("test", true);
            customShrinkage.AddAbsence(AbsenceFactory.CreateAbsence("for test"));
            budgetGroup.AddCustomShrinkage(customShrinkage);
            personPeriod.BudgetGroup = budgetGroup;

            _uowFactory.Stub(x => x.CurrentUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());

            _target.Initialize(budgetGroup, new DateOnly(2011, 12, 20));
            _target.SelectedBudgetGroup.CustomShrinkages.Count().Should().Be.EqualTo(1);
        }

        [Test, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ShouldLoadModel()
        {
            var budgetGroup = new BudgetGroup();
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());
            var personPeriod = new PersonPeriod(new DateOnly(2011, 12, 19),
                                                  new PersonContract(new Contract("Permanent"),
                                                                     new PartTimePercentage("50"),
                                                                     new ContractSchedule("Test")), new Team());

            var customShrinkage = new CustomShrinkage("test", true);
            var absence = AbsenceFactory.CreateAbsence("for test");
            absence.SetId(Guid.NewGuid());
            customShrinkage.AddAbsence(absence);
            budgetGroup.AddCustomShrinkage(customShrinkage);
            personPeriod.BudgetGroup = budgetGroup;

            _uowFactory.Stub(x => x.CurrentUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());

            _scheduleProjPersister.Stub(x => x.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(new DateOnly(2011, 12, 19), new DateOnly(2011, 12, 25)), budgetGroup, new Scenario("test"))).IgnoreArguments().Return(new List<PayloadWorkTime>());

            _target.Initialize(budgetGroup, new DateOnly(2011, 12, 20));
            _target.ReloadModel(new DateOnlyPeriod(new DateOnly(2011, 12, 19), new DateOnly(2011, 12, 25)), true);
            _target.AbsencesInBudgetGroup.Count.Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldHaveEmptyBudgetGroup()
        {
            var budgetGroup = new BudgetGroup {Name = "Test Budget Group"};
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(null);
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());
            _scheduleProjPersister.Stub(
                x =>
                x.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(new DateOnly(2011, 12, 19), new DateOnly(2011, 12, 25)),
                                            budgetGroup, new Scenario("test"))).IgnoreArguments().Return(
                                                new List<PayloadWorkTime>());
            _target.Initialize(null, new DateOnly(2011, 12, 19));
            _target.ReloadModel(new DateOnlyPeriod(new DateOnly(2011, 12, 19), new DateOnly(2011, 12, 25)), true);

            _target.SelectedBudgetGroup.Should().Be.InstanceOf<EmptyBudgetGroup>();
	        _target.SelectedBudgetGroup.Name.Should().Be.EqualTo(Resources.Empty);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldHaveLatestOneAbsence()
        {
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2011, 12, 19));
            var requestedDate = new DateTime(2011, 12, 15);
        	var scenario = ScenarioFactory.CreateScenarioAggregate();
            var request = MockRepository.GenerateMock<IPersonRequest>();
            request.Stub(x => x.Request).Return(new AbsenceRequest(AbsenceFactory.CreateAbsence("Holiday"),
                                                                   new DateTimePeriod(2011, 12, 20, 2011, 12, 20)));
            var person = PersonFactory.CreatePerson();
            person.AddPersonPeriod(personPeriod);
            request.Stub(x => x.Person).Return(person);
            request.Stub(x => x.RequestedDate).Return(requestedDate);

            var budgetGroup = new BudgetGroup();
            _budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { budgetGroup });
            _budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(
                new List<IBudgetDay> {new BudgetDay(budgetGroup, scenario, new DateOnly(requestedDate))});

            var customShrinkage = new CustomShrinkage("test", true);
            customShrinkage.AddAbsence(AbsenceFactory.CreateAbsence("for test"));
            budgetGroup.AddCustomShrinkage(customShrinkage);
            personPeriod.BudgetGroup = budgetGroup;

            _uowFactory.Stub(x => x.CurrentUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
            _scheduleProjPersister.Stub(
                x =>
                x.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(new DateOnly(2011, 12, 19), new DateOnly(2011, 12, 25)),
                                            budgetGroup, scenario)).IgnoreArguments().Return(
                                                new List<PayloadWorkTime>());

            _target.Initialize(budgetGroup, new DateOnly(2011, 12, 19));
            _target.ReloadModel(new DateOnlyPeriod(new DateOnly(2011, 12, 19), new DateOnly(2011, 12, 25)), true);
            _target.AbsencesInBudgetGroup.Count.Should().Be.EqualTo(1);
        }
    }
}
