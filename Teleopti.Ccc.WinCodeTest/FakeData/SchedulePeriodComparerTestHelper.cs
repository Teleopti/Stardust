using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.FakeData
{
    class SchedulePeriodComparerTestHelper
    {

		public ISchedulePeriod _schedulePeriod1, _schedulePeriod2,
            _schedulePeriod4, _schedulePeriod5;
        public IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3,
            _personPeriod4, _personPeriod5, _personPeriod6;
        public ISkill _skill1, _skill2, _skill3, _skill4, _skill5, _skill6;
        public IPerson person, person1;

        public DateOnly universalTime1 = new DateOnly(2010, 10, 09);
        public DateOnly universalTime2 = new DateOnly(2058, 10, 09);
        public DateOnly universalTime3 = new DateOnly(2008, 10, 09);

        public void SetFirstTarget()
        {
            person = PersonFactory.CreatePerson("Test");

            DateOnly from1 = new DateOnly(2008, 1, 3);
            DateOnly from2 = new DateOnly(2010, 1, 3);

            _schedulePeriod1 = SchedulePeriodFactory.CreateSchedulePeriod(from1);
            _schedulePeriod2 = SchedulePeriodFactory.CreateSchedulePeriod(from2);

            _skill1 = SkillFactory.CreateSkill("_skill1");
            _skill2 = SkillFactory.CreateSkill("_skill2");
            _skill3 = SkillFactory.CreateSkill("_skill3");

            _personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkillsWithSite(universalTime1, _skill1);
            _personPeriod2 = PersonPeriodFactory.CreatePersonPeriodWithSkillsWithSite(universalTime2, _skill2);
            _personPeriod3 = PersonPeriodFactory.CreatePersonPeriodWithSkillsWithSite(universalTime3, _skill3);

            _personPeriod1.RuleSetBag = new RuleSetBag();
            _personPeriod2.RuleSetBag = new RuleSetBag();
            _personPeriod3.RuleSetBag = new RuleSetBag();

            person.AddPersonPeriod(_personPeriod1);
            person.AddPersonPeriod(_personPeriod2);
            person.AddPersonPeriod(_personPeriod3);

        }

        public void SetSecondtarget()
        {
            person1 = PersonFactory.CreatePerson("Test");

            DateOnly from1 = new DateOnly(2008, 1, 3);
            DateOnly from2 = new DateOnly(2010, 1, 3);

            _schedulePeriod4 = SchedulePeriodFactory.CreateSchedulePeriod(from1);
            _schedulePeriod5 = SchedulePeriodFactory.CreateSchedulePeriod(from2);

            _skill4 = SkillFactory.CreateSkill("_skill4");
            _skill5 = SkillFactory.CreateSkill("_skill5");
            _skill6 = SkillFactory.CreateSkill("_skill6");

            _personPeriod4 = PersonPeriodFactory.CreatePersonPeriodWithSkillsWithSite(universalTime1, _skill4);
            _personPeriod5 = PersonPeriodFactory.CreatePersonPeriodWithSkillsWithSite(universalTime2, _skill5);
            _personPeriod6 = PersonPeriodFactory.CreatePersonPeriodWithSkillsWithSite(universalTime3, _skill6);

            _personPeriod4.RuleSetBag = new RuleSetBag();
            _personPeriod5.RuleSetBag = new RuleSetBag();
            _personPeriod6.RuleSetBag = new RuleSetBag();

            person1.AddPersonPeriod(_personPeriod4);
            person1.AddPersonPeriod(_personPeriod5);
            person1.AddPersonPeriod(_personPeriod6);
        }
    }
}
