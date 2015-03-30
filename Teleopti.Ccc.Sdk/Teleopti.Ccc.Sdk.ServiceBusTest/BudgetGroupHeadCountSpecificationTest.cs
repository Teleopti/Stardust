using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [TestFixture]
    public class BudgetGroupHeadCountSpecificationTest
    {
        private IScenarioRepository _scenarioRepository;
        private IBudgetDayRepository _budgetDayRepository;
        private IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private IBudgetGroupHeadCountSpecification _target;
        private IPerson _person;
        private readonly DateOnly _defaultDay = new DateOnly();
        private readonly DateOnlyPeriod _defaultDatePeriod = new DateOnlyPeriod();
	    private IScenario _scenario;

	    [SetUp]
        public void Setup()
        {
            _scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
            _budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();
            _scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
	        _scenario = ScenarioFactory.CreateScenarioAggregate();
            _person = PersonFactory.CreatePersonWithBasicPermissionInfo("billg", "billg");
            
            _target = new BudgetGroupHeadCountSpecification(_scenarioRepository, _budgetDayRepository,  
                                                            _scheduleProjectionReadOnlyRepository);
        }

	    [Test]
	    public void ShouldBeValidIfEnoughAllowanceLeft()
	    {
		    var budgetGroup = GetBudgetGroup();
		    var budgetDay = new BudgetDay(budgetGroup, _scenario, _defaultDay){Allowance = 2d,IsClosed = false};
		    var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);

		    personPeriod.BudgetGroup = budgetGroup;
		    _person.AddPersonPeriod(personPeriod);
		    _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
		    var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Vacation"), shortPeriod());
		    var personRequest = new PersonRequest(_person, absenceRequest);
		    personRequest.SetId(Guid.NewGuid());

			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_scenario);
			_budgetDayRepository.Stub(x => x.Find(null, null, _defaultDatePeriod)).IgnoreArguments().Return(new List<IBudgetDay> { budgetDay });
			_scheduleProjectionReadOnlyRepository.Stub(x => x.GetNumberOfAbsencesPerDayAndBudgetGroup(budgetGroup.Id.GetValueOrDefault(), personPeriod.StartDate)).Return(0);

		    Assert.IsTrue(_target.IsSatisfied(absenceRequest).IsValid);
	    }

	    [Test]
	    public void ShouldBeInvalidIfNotEnoughAllowanceLeft()
	    {
		    var budgetGroup = GetBudgetGroup();
		    var budgetDay = new BudgetDay(budgetGroup, _scenario, _defaultDay) {IsClosed = false, Allowance = 1.5d};
		    var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);

		    personPeriod.BudgetGroup = budgetGroup;
		    _person.AddPersonPeriod(personPeriod);
		    _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
			var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Vacation"), longPeriod());
		    var personRequest = new PersonRequest(_person, absenceRequest);
		    personRequest.SetId(Guid.NewGuid());

			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_scenario);
			_budgetDayRepository.Stub(x => x.Find(null, null, _defaultDatePeriod)).IgnoreArguments().Return(new List<IBudgetDay> { budgetDay });
			_scheduleProjectionReadOnlyRepository.Stub(x => x.GetNumberOfAbsencesPerDayAndBudgetGroup(budgetGroup.Id.GetValueOrDefault(), personPeriod.StartDate)).Return(1);
			
		    Assert.IsFalse(_target.IsSatisfied(absenceRequest).IsValid);
	    }

	    [Test]
	    public void ShouldBeInvalidIfAgentHasNoPersonPeriod()
	    {
		    _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);

		    var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Vacation"), longPeriod());
		    var personRequest = new PersonRequest(_person, absenceRequest);
		    personRequest.SetId(Guid.NewGuid());

		    Assert.IsFalse(_target.IsSatisfied(absenceRequest).IsValid);
	    }

	    [Test]
	    public void ShouldBeInvalidIfAgentDoesNotBelongToAnyBudgetGroup()
	    {
		    _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
		    var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
		    _person.AddPersonPeriod(personPeriod);
		    var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Vacation"), longPeriod());
		    var personRequest = new PersonRequest(_person, absenceRequest);
		    personRequest.SetId(Guid.NewGuid());

		    Assert.IsFalse(_target.IsSatisfied(absenceRequest).IsValid);
	    }

	    [Test]
	    public void ShouldReturnFalseIfThereIsNoBudgetDays()
	    {
		    var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
		    personPeriod.BudgetGroup = new BudgetGroup {Name = "BG1"};
		    personPeriod.BudgetGroup.SetId(new Guid());
		    _person.AddPersonPeriod(personPeriod);
		    _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);

			var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Vacation"), longPeriod());
		    var personRequest = new PersonRequest(_person, absenceRequest);
		    personRequest.SetId(Guid.NewGuid());

		    _scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_scenario);
		    
		    Assert.IsFalse(_target.IsSatisfied(absenceRequest).IsValid);
	    }

	    [Test]
	    public void ShouldReturnFalseIfBudgetIsNotDefinedForFewDays()
	    {
			var budgetGroup = GetBudgetGroup();
			var budgetDay = new BudgetDay(budgetGroup, _scenario, _defaultDay);
		    var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);

		    personPeriod.BudgetGroup = budgetGroup;
		    _person.AddPersonPeriod(personPeriod);
		    _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);

		    var budgetDayList = new List<IBudgetDay> {budgetDay};
		    var dateTimePeriod = new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
			    new DateTime(2011, 12, 3, 13, 0, 0, DateTimeKind.Utc));

		    var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Vacation"), dateTimePeriod);
		    var personRequest = new PersonRequest(_person, absenceRequest);
		    personRequest.SetId(Guid.NewGuid());

			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_scenario);
			_budgetDayRepository.Stub(x => x.Find(null, null, _defaultDatePeriod)).IgnoreArguments().Return(budgetDayList);
		    
		    Assert.IsFalse(_target.IsSatisfied(absenceRequest).IsValid);
	    }

	    private static IBudgetGroup GetBudgetGroup()
        {
            var budgetGroup = new BudgetGroup{Name = "BG1"};
            budgetGroup.SetId(Guid.NewGuid());
            return budgetGroup;
        }

        private static DateTimePeriod longPeriod()
        {
            return new DateTimePeriod(new DateTime(2016, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                      new DateTime(2016, 12, 1, 14, 1, 0, DateTimeKind.Utc));
        }

        private static DateTimePeriod shortPeriod()
        {
            return new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                      new DateTime(2011, 12, 1, 13, 0, 0, DateTimeKind.Utc));
        }
    }
}
