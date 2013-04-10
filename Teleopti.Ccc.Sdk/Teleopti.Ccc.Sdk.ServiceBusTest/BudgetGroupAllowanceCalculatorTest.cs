using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [TestFixture] 
    public class BudgetGroupAllowanceCalculatorTest
    {

        private MockRepository _mocks;
        private IScenarioRepository _scenarioRepository;
        private IBudgetDayRepository _budgetDayRepository;
        private IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private IBudgetGroupAllowanceCalculator _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
    	private IPerson _person;
    	private DateOnly _defaultDay;
    	private DateOnlyPeriod _defaultDatePeriod;
    	private IScheduleDictionary _scheduleDict;
    	private IScheduleDay _scheduleDay;
    	private IProjectionService _projectionService;
    	private IVisualLayerCollection _visualLayerCollection;
    	private IAbsenceRequest _absenceRequest;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scenarioRepository = _mocks.StrictMock<IScenarioRepository>();
            _budgetDayRepository = _mocks.StrictMock<IBudgetDayRepository>();
            _scheduleProjectionReadOnlyRepository = _mocks.StrictMock<IScheduleProjectionReadOnlyRepository>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_scheduleDict = _mocks.StrictMock<IScheduleDictionary>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_projectionService = _mocks.StrictMock<IProjectionService>();
			_visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
			_absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
			
			_person = PersonFactory.CreatePersonWithBasicPermissionInfo("billg", "billg");
			_defaultDay = new DateOnly();
			_defaultDatePeriod = new DateOnlyPeriod();
			
            _target = new BudgetGroupAllowanceCalculator(_schedulingResultStateHolder, _scenarioRepository, _budgetDayRepository,
                                                            _scheduleProjectionReadOnlyRepository);
        }
        
        [Test]
        public void ShouldBeEmptyStringIfEnoughAllowanceLeft()
        {
            var budgetDay = _mocks.StrictMock<IBudgetDay>();
            var usedAbsenceTime = new List<PayloadWorkTime> { new PayloadWorkTime { TotalContractTime = TimeSpan.FromHours(14).Ticks } };
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
            personPeriod.BudgetGroup = new BudgetGroup { Name = "BG1" };
            _person.AddPersonPeriod(personPeriod);
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                getExpectIfEnoughAllowanceLeft(budgetDay, usedAbsenceTime);
            }
            using (_mocks.Playback())
            {
                Assert.IsNullOrEmpty(_target.CheckBudgetGroup(_absenceRequest));
            }
        }

        private void getExpectIfEnoughAllowanceLeft(IBudgetDay budgetDay, IEnumerable<PayloadWorkTime> usedAbsenceTime)
        {
            Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(ScenarioFactory.CreateScenarioAggregate());
            Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Period).Return(shortPeriod()).Repeat.AtLeastOnce();
            Expect.Call(_budgetDayRepository.Find(null, null, _defaultDatePeriod))
                  .IgnoreArguments()
                  .Return(new List<IBudgetDay> {budgetDay});
            Expect.Call(budgetDay.Allowance).Return(2d);
            Expect.Call(budgetDay.Day).Return(_defaultDay).Repeat.Twice();
            Expect.Call(budgetDay.FulltimeEquivalentHours).Return(8d);
            Expect.Call(_scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(_defaultDatePeriod, null, null))
                  .IgnoreArguments()
                  .
                   Return(usedAbsenceTime);
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDict);
            Expect.Call(_scheduleDict[_person].ScheduledDay(_defaultDay)).IgnoreArguments().Return(_scheduleDay);
            Expect.Call(_scheduleDay.IsScheduled()).Return(true);
            Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
            Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
            Expect.Call(_visualLayerCollection.Period()).Return(null);
        }

        private static DateTimePeriod shortPeriod()
        {
            return new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                      new DateTime(2011, 12, 1, 13, 0, 0, DateTimeKind.Utc));
        }

        private static DateTimePeriod longPeriod()
        {
            return new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                      new DateTime(2011, 12, 1, 14, 1, 0, DateTimeKind.Utc));
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        [Test]
        public void ShouldBeInvalidIfNotEnoughAllowanceLeft()
        {
            var budgetDay = _mocks.StrictMock<IBudgetDay>();

            var usedAbsenceTime = new List<PayloadWorkTime> { new PayloadWorkTime { TotalContractTime = TimeSpan.FromHours(14).Ticks } };
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
            personPeriod.BudgetGroup = new BudgetGroup { Name = "BG1" };
            _person.AddPersonPeriod(personPeriod);
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                getExpectCallsIfNotEnoughAllowanceLeft(budgetDay, usedAbsenceTime);
            }
            using (_mocks.Playback())
            {
                Assert.IsNotNullOrEmpty(_target.CheckBudgetGroup(_absenceRequest));
            }
        }

        private void getExpectCallsIfNotEnoughAllowanceLeft(IBudgetDay budgetDay, IEnumerable<PayloadWorkTime> usedAbsenceTime)
        {
            Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(ScenarioFactory.CreateScenarioAggregate());
            Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Period).Return(longPeriod()).Repeat.AtLeastOnce();
            Expect.Call(_budgetDayRepository.Find(null, null, _defaultDatePeriod))
                  .IgnoreArguments()
                  .Return(new List<IBudgetDay> {budgetDay});
            Expect.Call(budgetDay.Day).Return(_defaultDay).Repeat.Times(4);
            Expect.Call(budgetDay.Allowance).Return(2d);
            Expect.Call(budgetDay.FulltimeEquivalentHours).Return(8d);
            Expect.Call(_scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(_defaultDatePeriod, null, null))
                  .IgnoreArguments()
                  .
                   Return(usedAbsenceTime);
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDict);
            Expect.Call(_scheduleDict[_person].ScheduledDay(_defaultDay)).IgnoreArguments().Return(_scheduleDay);
            Expect.Call(_scheduleDay.IsScheduled()).Return(true);
            Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
            Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
            Expect.Call(_visualLayerCollection.Period()).Return(null);
        }

        [Test]
        public void ShouldBeInvalidIfAgentHasNoPersonPeriod()
        {
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_absenceRequest.Period).Return(longPeriod()).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                Assert.IsNotNullOrEmpty(_target.CheckBudgetGroup(_absenceRequest));
            }
        }

        [Test]
        public void ShouldBeInvalidIfAgentDoesNotBelongToAnyBudgetGroup()
        {
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
            _person.AddPersonPeriod(personPeriod);
            using (_mocks.Record())
            {
                Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_absenceRequest.Period).Return(longPeriod()).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                Assert.IsNotNullOrEmpty(_target.CheckBudgetGroup(_absenceRequest));
            }
        }

    }
}
