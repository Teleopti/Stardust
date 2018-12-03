using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.TestCommon
{
    public class BaseLineData
    {
        private readonly DateOnly _dateOnly;

        public BaseLineData(DateOnly dateOnly )
        {
            _dateOnly = dateOnly;
            initValues();
        }

        public IPerson Person1 { get; set; }
        public IPerson Person2 { get; set; }
        public IList<IPerson> PersonList { get; set; }
        public ReadOnlyCollection<IPerson> ReadOnlyCollectionPersonList { get; set; }

        public Group Group { get; set; }


        public SchedulingOptions SchedulingOptions { get; set; }

        public DateOnly BaseDateOnly { get; set; }

        public IGroupPageOptions GroupPageOptions { get; set; }

        public IScenario Scenario { get; set; }

        public ISkill SampleSkill { get; set; }

        public IPersonSkill SamplePersonSkill { get; set; }

        public IRuleSetBag SampleRuleSetBag { get; set; }

        public IActivity Activity1 { get; set; }
        public IActivity Activity2 { get; set; }

        public IBlockInfo BlockOfThreeDays { get; set; }

        private void initValues()
        {
            Person1 = PersonFactory.CreatePerson();
            Person2 = PersonFactory.CreatePerson();
            PersonList = new List<IPerson> { Person1, Person2 };
            ReadOnlyCollectionPersonList = new ReadOnlyCollection<IPerson>(PersonList);
            Activity1 = ActivityFactory.CreateActivity("Activity1", new Color());
            Activity2 = ActivityFactory.CreateActivity("Activity2", new Color());

            BaseDateOnly = DateOnly.Today;

            SampleSkill = SkillFactory.CreateSkillWithWorkloadAndSources();
            SamplePersonSkill = PersonSkillFactory.CreatePersonSkillWithSamePercent(SampleSkill);
            SchedulingOptions = new SchedulingOptions();
            SampleRuleSetBag = new RuleSetBag();
            GroupPageOptions = new GroupPageOptions(PersonList);
            Scenario = new Scenario("test");
            BlockOfThreeDays  = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(3)));
			Group = new Group(new List<IPerson>{Person1, Person2}, "Kalle");
        }

        public BaseLineData()
        {
            _dateOnly = DateOnly.Today;
            initValues();
        }
    }
}
