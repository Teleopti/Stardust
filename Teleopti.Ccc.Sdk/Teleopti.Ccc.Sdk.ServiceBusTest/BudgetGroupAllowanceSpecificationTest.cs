using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [TestFixture]
    public class BudgetGroupAllowanceSpecificationTest
    {
        private MockRepository _mocks;
        private IScenarioProvider _scenarioProvider;
        private IBudgetDayRepository _budgetDayRepository;
        private IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private IBudgetGroupAllowanceSpecification _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scenarioProvider = _mocks.StrictMock<IScenarioProvider>();
            _budgetDayRepository = _mocks.StrictMock<IBudgetDayRepository>();
            _scheduleProjectionReadOnlyRepository = _mocks.StrictMock<IScheduleProjectionReadOnlyRepository>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _target = new BudgetGroupAllowanceSpecification(_schedulingResultStateHolder, _scenarioProvider, _budgetDayRepository,
                                                            _scheduleProjectionReadOnlyRepository);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldBeValidIfEnoughAllowanceLeft()
        {
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            var budgetDay = _mocks.StrictMock<IBudgetDay>();
            var scheduleDict = _mocks.StrictMock<IScheduleDictionary>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var usedAbsenceTime = new List<PayloadWorkTime> { new PayloadWorkTime { TotalContractTime = TimeSpan.FromHours(14).Ticks } };
            var person = PersonFactory.CreatePersonWithBasicPermissionInfo("billg", "billg");
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());
            personPeriod.BudgetGroup = new BudgetGroup { Name = "BG1" };
            person.AddPersonPeriod(personPeriod);
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                Expect.Call(_scenarioProvider.DefaultScenario()).Return(ScenarioFactory.CreateScenarioAggregate());
                Expect.Call(absenceRequest.Person).Return(person).Repeat.Any();
                Expect.Call(absenceRequest.Period).Return(new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                                                             new DateTime(2011, 12, 1, 13, 0, 0, DateTimeKind.Utc))).Repeat.Any();
                Expect.Call(_budgetDayRepository.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay> { budgetDay });
                Expect.Call(budgetDay.Allowance).Return(2d);
                Expect.Call(budgetDay.Day).Return(new DateOnly()).Repeat.Once();
                Expect.Call(budgetDay.FulltimeEquivalentHours).Return(8d);
                Expect.Call(_scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(), null,
                                                                                            null)).IgnoreArguments().
                    Return(usedAbsenceTime);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDict);
                Expect.Call(scheduleDict[person].ScheduledDay(new DateOnly())).IgnoreArguments().Return(scheduleDay);
                Expect.Call(scheduleDay.IsScheduled()).Return(true);
                Expect.Call(scheduleDay.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
                Expect.Call(visualLayerCollection.Period()).Return(null);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(absenceRequest));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldBeInvalidIfNotEnoughAllowanceLeft()
        {
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            var budgetDay = _mocks.StrictMock<IBudgetDay>();
            var scheduleDict = _mocks.StrictMock<IScheduleDictionary>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var usedAbsenceTime = new List<PayloadWorkTime> { new PayloadWorkTime { TotalContractTime = TimeSpan.FromHours(14).Ticks } };
            var person = PersonFactory.CreatePersonWithBasicPermissionInfo("billg", "billg");
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());
            personPeriod.BudgetGroup = new BudgetGroup { Name = "BG1" };
            person.AddPersonPeriod(personPeriod);
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                Expect.Call(_scenarioProvider.DefaultScenario()).Return(ScenarioFactory.CreateScenarioAggregate());
                Expect.Call(absenceRequest.Person).Return(person).Repeat.Any();
                Expect.Call(absenceRequest.Period).Return(new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                                                             new DateTime(2011, 12, 1, 14, 1, 0, DateTimeKind.Utc))).Repeat.Any();
                Expect.Call(_budgetDayRepository.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>{budgetDay});
                Expect.Call(budgetDay.Day).Return(new DateOnly()).Repeat.Twice();
                Expect.Call(budgetDay.Allowance).Return(2d);
                Expect.Call(budgetDay.FulltimeEquivalentHours).Return(8d);
                Expect.Call(_scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(), null,
                                                                                            null)).IgnoreArguments().
                    Return(usedAbsenceTime);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDict);
                Expect.Call(scheduleDict[person].ScheduledDay(new DateOnly())).IgnoreArguments().Return(scheduleDay);
                Expect.Call(scheduleDay.IsScheduled()).Return(true);
                Expect.Call(scheduleDay.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
                Expect.Call(visualLayerCollection.Period()).Return(null);
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(absenceRequest));
            }
        }

        [Test]
        public void ShouldBeInvalidIfAgentHasNoPersonPeriod()
        {
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            var person = PersonFactory.CreatePersonWithBasicPermissionInfo("billg", "billg");
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                Expect.Call(absenceRequest.Person).Return(person).Repeat.Any();
                Expect.Call(absenceRequest.Period).Return(
                    new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 12, 1, 14, 1, 0, DateTimeKind.Utc))).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(absenceRequest));
            }
        }

        [Test]
        public void ShouldBeInvalidIfAgentDoesNotBelongToAnyBudgetGroup()
        {
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            var person = PersonFactory.CreatePersonWithBasicPermissionInfo("billg", "billg");
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());
            person.AddPersonPeriod(personPeriod);
            using (_mocks.Record())
            {
                Expect.Call(absenceRequest.Person).Return(person).Repeat.Any();
                Expect.Call(absenceRequest.Period).Return(
                    new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 12, 1, 14, 1, 0, DateTimeKind.Utc))).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(absenceRequest));
            }
        }
    }
}
