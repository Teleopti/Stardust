using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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
                  .Return(usedAbsenceTime);
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

	    [Test]
	    public void CheckHeadCount_NoPersonPeriod()
	    {
			_person.SetId(Guid.NewGuid());
			_person.RemoveAllPersonPeriods();

		    _absenceRequest.Expect(a => a.Person).Return(_person).Repeat.Any();
		    _absenceRequest.Expect(a => a.Period).Return(longPeriod());
			_mocks.ReplayAll();

		    var result = _target.CheckHeadCountInBudgetGroup(_absenceRequest);
		    result.Should().Contain(_person.Id.GetValueOrDefault().ToString());
			_mocks.VerifyAll();
	    }

		[Test]
		public void CheckHeadCount_NoBudgetGroup()
		{
			var personPeriod = MockRepository.GenerateMock<PersonPeriod>();
			
			_person.AddPersonPeriod(personPeriod);
			_person.SetId(Guid.NewGuid());
			
			_absenceRequest.Expect(a => a.Person).Return(_person).Repeat.Any();
			_absenceRequest.Expect(a => a.Period).Return(longPeriod());
			_mocks.ReplayAll();

			var result = _target.CheckHeadCountInBudgetGroup(_absenceRequest);
			result.Should().Contain(_person.Id.GetValueOrDefault().ToString());
			_mocks.VerifyAll();
		}

		[Test]
		public void CheckHeadCount_NoBudgetDays()
		{
			var scenario = new Scenario("Test");
			var budgetGroup = new BudgetGroup();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(longPeriod().StartDateTime));
			
			personPeriod.BudgetGroup = budgetGroup;
			_person.AddPersonPeriod(personPeriod);
			
			_absenceRequest.Expect(a => a.Person).Return(_person).Repeat.Any();
			_absenceRequest.Expect(a => a.Period).Return(longPeriod());
			_scenarioRepository.Expect(s => s.LoadDefaultScenario()).Return(scenario);
			_budgetDayRepository.Expect(
				b => b.Find(scenario, budgetGroup, longPeriod().ToDateOnlyPeriod(TimeZoneHelper.CurrentSessionTimeZone))).Return(null);
			_mocks.ReplayAll();

			var result = _target.CheckHeadCountInBudgetGroup(_absenceRequest);
			result.Should().Contain(longPeriod().ToDateOnlyPeriod(TimeZoneHelper.CurrentSessionTimeZone).ToString());
			_mocks.VerifyAll();
		}

		[Test]
		public void CheckHeadCount_BudgetDaysNotEqualToDayCollection()
		{
			var period = new DateTimePeriod(2013, 05, 21, 2013, 05, 21);
			var scenario = new Scenario("Test");
			var budgetGroup = new BudgetGroup();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(period.StartDateTime));
			
			personPeriod.BudgetGroup = budgetGroup;
			_person.AddPersonPeriod(personPeriod);

			_absenceRequest.Expect(a => a.Person).Return(_person).Repeat.Any();
			_absenceRequest.Expect(a => a.Period).Return(period);
			_scenarioRepository.Expect(s => s.LoadDefaultScenario()).Return(scenario);
			_budgetDayRepository.Expect(
				b => b.Find(scenario, budgetGroup, period.ToDateOnlyPeriod(TimeZoneHelper.CurrentSessionTimeZone)))
			                    .Return(new List<IBudgetDay>
				                    {
					                    new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 05, 21)),
										new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 05, 22))
				                    });
			_mocks.ReplayAll();

			var result = _target.CheckHeadCountInBudgetGroup(_absenceRequest);
			result.Should().Contain(period.ToDateOnlyPeriod(TimeZoneHelper.CurrentSessionTimeZone).ToString());
			_mocks.VerifyAll();
		}

		[Test]
		public void CheckHeadCount_InvalidDays()
		{
			var period = new DateTimePeriod(2013, 05, 21, 2013, 05, 21);
			var scenario = new Scenario("Test");
			var budgetGroup = new BudgetGroup();
			var budgetDay = new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 05, 21));
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(period.StartDateTime));
			
			personPeriod.BudgetGroup = budgetGroup;
			budgetGroup.SetId(Guid.NewGuid());
			budgetDay.Allowance = 1.5;
			_person.AddPersonPeriod(personPeriod);

			_absenceRequest.Expect(a => a.Person).Return(_person).Repeat.Any();
			_absenceRequest.Expect(a => a.Period).Return(period);
			_scenarioRepository.Expect(s => s.LoadDefaultScenario()).Return(scenario);
			_budgetDayRepository.Expect(
				b => b.Find(scenario, budgetGroup, period.ToDateOnlyPeriod(TimeZoneHelper.CurrentSessionTimeZone)))
			                    .Return(new List<IBudgetDay> {budgetDay});
			_scheduleProjectionReadOnlyRepository.Expect(
				s => s.GetNumberOfAbsencesPerDayAndBudgetGroup(budgetDay.BudgetGroup.Id.GetValueOrDefault(), budgetDay.Day))
												 .Return(1);
			_mocks.ReplayAll();

			var result = _target.CheckHeadCountInBudgetGroup(_absenceRequest);
			result.Should().Contain(period.StartDateTime.Date.ToString("d" ,CultureInfo.CurrentCulture));
			_mocks.VerifyAll();
		}

		[Test]
		public void CheckHeadCount_ForOneWeek()
		{
			var period = new DateTimePeriod(2013, 05, 21, 2013, 05, 27);
			var dateOnly = new DateOnly(2013, 05, 22);
			var scenario = new Scenario("Test");
			var budgetGroup = new BudgetGroup();
			var budgetDay = new BudgetDay(budgetGroup, scenario, dateOnly);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(period.StartDateTime));
			var budgetDays = new List<IBudgetDay> { budgetDay };
			
			personPeriod.BudgetGroup = budgetGroup;
			budgetGroup.SetId(Guid.NewGuid());
			_person.AddPersonPeriod(personPeriod);
			for (var i = 0; i < 5; i++)
				budgetDays.Add(new BudgetDay(budgetGroup, scenario, dateOnly.AddDays(i)));

			_absenceRequest.Expect(a => a.Person).Return(_person).Repeat.Any();
			_absenceRequest.Expect(a => a.Period).Return(period);
			_scenarioRepository.Expect(s => s.LoadDefaultScenario()).Return(scenario);
			_budgetDayRepository.Expect(
				b => b.Find(scenario, budgetGroup, new DateOnlyPeriod(2013, 05, 21, 2013, 05, 26)))
			                    .Return(budgetDays);
			budgetDays.ForEach(d =>
				_scheduleProjectionReadOnlyRepository.Expect(
					s => s.GetNumberOfAbsencesPerDayAndBudgetGroup(d.BudgetGroup.Id.GetValueOrDefault(), d.Day)).Return(1));

			_mocks.ReplayAll();

			var result = _target.CheckHeadCountInBudgetGroup(_absenceRequest);
			budgetDays.Take(5).ForEach(
				d => result.Should().Contain(d.Day.Date.ToString("d", CultureInfo.CurrentCulture)));
			_mocks.VerifyAll();
		}	
	}
}
