using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [TestFixture]
    public class BudgetGroupAllowanceSpecificationTest
    {
        private MockRepository _mocks;
        private ICurrentScenario _scenarioRepository;
        private IBudgetDayRepository _budgetDayRepository;
        private IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private IBudgetGroupAllowanceSpecification _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
    	private IPerson _person;
    	private DateOnly _defaultDay;
    	private DateOnlyPeriod _defaultDatePeriod;
    	private IScheduleDictionary _scheduleDict;
    	private IScheduleDay _scheduleDay;
    	private IProjectionService _projectionService;
    	private IVisualLayerCollection _visualLayerCollection;
    	private IAbsenceRequest _absenceRequest;
        private IEnumerable<ISkill> _skills;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scenarioRepository = _mocks.StrictMock<ICurrentScenario>();
            _budgetDayRepository = _mocks.StrictMock<IBudgetDayRepository>();
            _scheduleProjectionReadOnlyRepository = _mocks.StrictMock<IScheduleProjectionReadOnlyRepository>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_scheduleDict = _mocks.StrictMock<IScheduleDictionary>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_projectionService = _mocks.StrictMock<IProjectionService>();
			_visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
			_absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            _skills = prepareSkillListOpenFromMondayToFriday();

			_person = PersonFactory.CreatePersonWithBasicPermissionInfo("billg", "billg");
			_defaultDay = new DateOnly();
			_defaultDatePeriod = new DateOnlyPeriod();
			
            _target = new BudgetGroupAllowanceSpecification(_schedulingResultStateHolder, _scenarioRepository, _budgetDayRepository,
                                                            _scheduleProjectionReadOnlyRepository);
        }

        [Test]
        public void ShouldReturnTrueIfSkillIsOpen()
        {
            // 2013-04-08 is a monday
            var result = BudgetGroupAllowanceSpecificationForTest.IsSkillOpenForDateOnlyForTest(new DateOnly(2013, 04, 08), _skills);
            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldReturnFalseIfSkillIsClosed()
        {
            // 2013-04-07 is a sunday        
            var result = BudgetGroupAllowanceSpecificationForTest.IsSkillOpenForDateOnlyForTest(new DateOnly(2013, 04, 07), _skills);
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldBeValidIfEnoughAllowanceLeft()
        {
            var budgetDay = _mocks.StrictMock<IBudgetDay>();
            var usedAbsenceTime = new List<PayloadWorkTime> { new PayloadWorkTime { TotalContractTime = TimeSpan.FromHours(14).Ticks } };
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);

            var budgetGroup = GetBudgetGroup();

            personPeriod.BudgetGroup = budgetGroup;
            _person.AddPersonPeriod(personPeriod);
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                Expect.Call(_scenarioRepository.Current()).Return(ScenarioFactory.CreateScenarioAggregate());
                Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_absenceRequest.Period).Return(shortPeriod()).Repeat.AtLeastOnce();
                Expect.Call(_budgetDayRepository.Find(null, null, _defaultDatePeriod)).IgnoreArguments().Return(new List<IBudgetDay> { budgetDay });
                Expect.Call(budgetDay.Allowance).Return(2d);
                Expect.Call(budgetDay.Day).Return(_defaultDay).Repeat.Times(2);
                Expect.Call(budgetDay.IsClosed).Return(false);
                Expect.Call(budgetDay.FulltimeEquivalentHours).Return(8d);
                Expect.Call(_scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(_defaultDatePeriod, null, null)).IgnoreArguments().
                    Return(usedAbsenceTime);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDict);
                Expect.Call(_scheduleDict[_person].ScheduledDay(_defaultDay)).IgnoreArguments().Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);
                Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.Period()).Return(null);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.IsSatisfied(_absenceRequest).IsValid);
            }
        }

        [Test]
        public void ShouldBeInvalidIfNotEnoughAllowanceLeft()
        {
            var budgetDay = _mocks.StrictMock<IBudgetDay>();
            
			var usedAbsenceTime = new List<PayloadWorkTime> { new PayloadWorkTime { TotalContractTime = TimeSpan.FromHours(14).Ticks } };
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
            var budgetGroup = GetBudgetGroup();
            
            personPeriod.BudgetGroup = budgetGroup;
            _person.AddPersonPeriod(personPeriod);
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                Expect.Call(_scenarioRepository.Current()).Return(ScenarioFactory.CreateScenarioAggregate());
                Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_absenceRequest.Period).Return(longPeriod()).Repeat.AtLeastOnce();
                Expect.Call(_budgetDayRepository.Find(null, null, _defaultDatePeriod)).IgnoreArguments().Return(new List<IBudgetDay>{budgetDay});
                Expect.Call(budgetDay.Day).Return(_defaultDay).Repeat.Times(4);
                Expect.Call(budgetDay.Allowance).Return(2d);
                Expect.Call(budgetDay.IsClosed).Return(false);
                Expect.Call(budgetDay.FulltimeEquivalentHours).Return(8d);
                Expect.Call(_scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(_defaultDatePeriod, null, null)).IgnoreArguments().
                    Return(usedAbsenceTime);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDict);
                Expect.Call(_scheduleDict[_person].ScheduledDay(_defaultDay)).IgnoreArguments().Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);
                Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.Period()).Return(null);
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfied(_absenceRequest).IsValid);
            }
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
                Assert.IsFalse(_target.IsSatisfied(_absenceRequest).IsValid);
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
                Assert.IsFalse(_target.IsSatisfied(_absenceRequest).IsValid);
            }
        }

        private static BudgetGroup GetBudgetGroup()
        {
            var budgetGroup = new BudgetGroup();
            var skill = SkillFactory.CreateSkill("Test Skill");
            var wl = WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

            foreach (var day in wl.TemplateWeekCollection)
            {
                day.Value.MakeOpen24Hours();
            }

            skill.AddWorkload(wl);
            budgetGroup.SetId(Guid.NewGuid());
            budgetGroup.Name = "BG1";
            budgetGroup.AddSkill(skill);
            return budgetGroup;
        }

        private static DateTimePeriod longPeriod()
        {
            return new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                      new DateTime(2011, 12, 1, 14, 1, 0, DateTimeKind.Utc));
        }

        private static DateTimePeriod shortPeriod()
        {
            return new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                                      new DateTime(2011, 12, 1, 13, 0, 0, DateTimeKind.Utc));
        }

        private static IEnumerable<ISkill> prepareSkillListOpenFromMondayToFriday()
        {
            var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
            var workload = WorkloadFactory.CreateWorkload(skill);

            for (var i = 1; i < 5; i++)
            {
                var dayTemplate = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, (DayOfWeek)i);
                dayTemplate.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) });
                workload.SetTemplateAt(i, dayTemplate);
            }

            return new List<ISkill> { skill };
        }
    }

    public class BudgetGroupAllowanceSpecificationForTest : BudgetGroupAllowanceSpecification
    {
        public BudgetGroupAllowanceSpecificationForTest(ISchedulingResultStateHolder schedulingResultStateHolder,
            ICurrentScenario scenarioRepository,IBudgetDayRepository budgetDayRepository,IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
            : base(schedulingResultStateHolder, scenarioRepository, budgetDayRepository, scheduleProjectionReadOnlyRepository)
        {
        }

        public static bool IsSkillOpenForDateOnlyForTest(DateOnly date, IEnumerable<ISkill> skills)
        {
            return IsSkillOpenForDateOnly(date, skills);
        }
    }


}
