using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
    public class BaseLineData
    {
        
        public IPerson Person1 { get; set; }
        public IPerson Person2 { get; set; }
        public IList<IPerson> PersonList { get; set; }
        public ReadOnlyCollection<IPerson> ReadOnlyCollectionPersonList { get; set; }

        public IGroupPerson GroupPerson { get; set; }

        public ISchedulingOptions SchedulingOptions { get; set; }

        public DateOnly BaseDateOnly { get; set; }

        public IGroupPageOptions GroupPageOptions { get; set; }

        public IScenario Scenario { get; set; }


        public BaseLineData()
        {
            Person1 = PersonFactory.CreatePerson();
            Person2  = PersonFactory.CreatePerson();
            PersonList = new List<IPerson>{Person1,Person2 };
            ReadOnlyCollectionPersonList = new ReadOnlyCollection<IPerson>(PersonList );

            BaseDateOnly = DateOnly.Today;
            
            var groupPersonFactory = new GroupPersonFactory();
            GroupPerson = groupPersonFactory.CreateGroupPerson(PersonList, BaseDateOnly, "GroupPerson", new Guid());
            
            SchedulingOptions = new SchedulingOptions();
            
            GroupPageOptions = new GroupPageOptions(PersonList );

            Scenario  = new Scenario("test");

        }


        public IGroupPerson GroupPersonOnDate(DateOnly dateOnly)
        {
            var groupPersonFactory = new GroupPersonFactory();
            GroupPerson = groupPersonFactory.CreateGroupPerson(PersonList, dateOnly, "GroupPerson", new Guid());
            return GroupPerson;
        }



    }
}
