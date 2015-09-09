using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount"), TestFixture]
    public class BudgetGroupHeadCountSpecificationTest
    {
        private MockRepository _mocks;
        private IScenarioRepository _scenarioRepository;
        private IBudgetDayRepository _budgetDayRepository;
        private IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private IBudgetGroupHeadCountSpecification _target;
        private IPerson _person;
        private DateOnly _defaultDay;
        private DateOnlyPeriod _defaultDatePeriod;
        private IAbsenceRequest _absenceRequest;
        private IEnumerable<ISkill> _skills;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scenarioRepository = _mocks.StrictMock<IScenarioRepository>();
            _budgetDayRepository = _mocks.StrictMock<IBudgetDayRepository>();
            _scheduleProjectionReadOnlyRepository = _mocks.StrictMock<IScheduleProjectionReadOnlyRepository>();
            _absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            _skills = prepareSkillListOpenFromMondayToFriday();
            _person = PersonFactory.CreatePersonWithBasicPermissionInfo("billg", "billg");
            _defaultDay = new DateOnly();
            _defaultDatePeriod = new DateOnlyPeriod();

            _target = new BudgetGroupHeadCountSpecification(_scenarioRepository, _budgetDayRepository,  
                                                            _scheduleProjectionReadOnlyRepository);
        }

        [Test]
        public void ShouldBeValidIfEnoughAllowanceLeft()
        {
            var budgetDay = _mocks.StrictMock<IBudgetDay>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
            var budgetGroup = GetBudgetGroup();

            personPeriod.BudgetGroup = budgetGroup;
            _person.AddPersonPeriod(personPeriod);
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
           
            using (_mocks.Record())
            {
                ExpectCallsForEnoughAllowanceLeft(budgetDay, personPeriod);
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
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
            var budgetGroup = GetBudgetGroup();

            personPeriod.BudgetGroup = budgetGroup;
            _person.AddPersonPeriod(personPeriod);
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);

            using (_mocks.Record())
            {
                GetExpectForNotEnoughAllowanceLeft(budgetDay, personPeriod);
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

        [Test]
        public void ShouldReturnFalseIfThereIsNoBudgetDays()
        {
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
            personPeriod.BudgetGroup = new BudgetGroup { Name = "BG1" };
            personPeriod.BudgetGroup.SetId(new Guid());
            _person.AddPersonPeriod(personPeriod);
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            using (_mocks.Record())
            {
                Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(ScenarioFactory.CreateScenarioAggregate());
                Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_absenceRequest.Period).Return(shortPeriod()).Repeat.AtLeastOnce();
                Expect.Call(_budgetDayRepository.Find(null, null, _defaultDatePeriod)).IgnoreArguments().Return(null);
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfied(_absenceRequest).IsValid);
            }
        }

        [Test]
        public void ShouldReturnFalseIfBudgetIsNotDefinedForFewDays()
        {
            var budgetDay = _mocks.StrictMock<IBudgetDay>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(_defaultDay);
            personPeriod.BudgetGroup = new BudgetGroup { Name = "BG1" };
            personPeriod.BudgetGroup.SetId(new Guid());
            _person.AddPersonPeriod(personPeriod);
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);

            var budgetDayList = new List<IBudgetDay> {budgetDay};
            var dateTimePeriod =  new DateTimePeriod(new DateTime(2011, 12, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2011, 12, 3, 13, 0, 0, DateTimeKind.Utc));

            using (_mocks.Record())
            {
                GetExpectIfBudgetIsNotDefinedForFewDays(dateTimePeriod, budgetDayList);
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

        private void ExpectCallsForEnoughAllowanceLeft(IBudgetDay budgetDay, IPersonPeriod personPeriod)
        {
            Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(ScenarioFactory.CreateScenarioAggregate()).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Period).Return(shortPeriod()).Repeat.AtLeastOnce();
            Expect.Call(_budgetDayRepository.Find(null, null, _defaultDatePeriod))
                  .IgnoreArguments()
                  .Return(new List<IBudgetDay> { budgetDay });
            Expect.Call(budgetDay.Allowance).Return(2d);
            Expect.Call(budgetDay.IsClosed).Return(false);
            Expect.Call(budgetDay.Day).Return(_defaultDay).Repeat.Twice();
            Expect.Call(
                _scheduleProjectionReadOnlyRepository.GetNumberOfAbsencesPerDayAndBudgetGroup(
                    personPeriod.BudgetGroup.Id.GetValueOrDefault(), new DateOnly(personPeriod.StartDate))).Return(0);

            ICollection<ISkillDay> skillDays = new Collection<ISkillDay>();
            var skillDay = SkillDayFactory.CreateSkillDay(new DateTime(2013, 04, 08));
            skillDay.SkillDayCalculator = new SkillDayCalculator(_skills.ElementAt(0), skillDays.ToList(), new DateOnlyPeriod(new DateOnly(2013, 04, 08), new DateOnly(2013, 04, 08)));
            skillDays.Add(skillDay);
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


        private void GetExpectForNotEnoughAllowanceLeft(IBudgetDay budgetDay, IPersonPeriod personPeriod)
        {
            Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(ScenarioFactory.CreateScenarioAggregate()).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Period).Return(longPeriod()).Repeat.AtLeastOnce();
            Expect.Call(_budgetDayRepository.Find(null, null, _defaultDatePeriod))
                  .IgnoreArguments()
                  .Return(new List<IBudgetDay> { budgetDay });
            Expect.Call(budgetDay.Day).Return(_defaultDay).Repeat.Times(2);
            Expect.Call(budgetDay.IsClosed).Return(false);
            Expect.Call(budgetDay.Allowance).Return(1.5d);
            Expect.Call(
                _scheduleProjectionReadOnlyRepository.GetNumberOfAbsencesPerDayAndBudgetGroup(
                    personPeriod.BudgetGroup.Id.GetValueOrDefault(), new DateOnly(personPeriod.StartDate))).Return(1);

            ICollection<ISkillDay> skillDays = new Collection<ISkillDay>();
            var skillDay = SkillDayFactory.CreateSkillDay(new DateTime(2013, 04, 08));
            skillDay.SkillDayCalculator = new SkillDayCalculator(_skills.ElementAt(0), skillDays.ToList(), new DateOnlyPeriod(new DateOnly(2013, 04, 08), new DateOnly(2013, 04, 08)));
            skillDays.Add(skillDay);
        }

        private void GetExpectIfBudgetIsNotDefinedForFewDays(DateTimePeriod dateTimePeriod, List<IBudgetDay> budgetDayList)
        {
            Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(ScenarioFactory.CreateScenarioAggregate());
            Expect.Call(_absenceRequest.Period).Return(dateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_budgetDayRepository.Find(null, null, _defaultDatePeriod)).IgnoreArguments().Return(budgetDayList);
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
}
