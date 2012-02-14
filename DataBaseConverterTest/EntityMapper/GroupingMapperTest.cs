using System;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests GroupingMapper
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/4/2008
    /// </remarks>
    [TestFixture]
    public class GroupingMapperTest
    {
        private global::Domain.Grouping _oldEntity;
        private IGroupPage _newEntity;
        private GroupingMapper _groupingMapper;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/5/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Agent = new ObjectPairCollection<Agent, IPerson>();

            //region setup old entity

            //Setup an agent
            global::Domain.DatePeriod datePeriod = new DatePeriod(new DateTime(2008,07,07));
            global::Domain.AgentPeriod agentPeriod = new AgentPeriod(datePeriod, -1,
                                                                     null, null, null, null, null, null, null, null,
                                                                     DateTime.Now);

            global::Domain.ICccListCollection<AgentPeriod> periodCollection =
                new global::Infrastructure.CccListCollection<AgentPeriod>();
            periodCollection.Add(agentPeriod);

            global::Domain.Agent agent = new Agent(-1, "Tom", "John", "john@xxx.zz", 
                                                    "TestSign", null, periodCollection, null, "TestNote");

            global::Domain.ICccListCollection<global::Domain.Agent> agents = 
                        new global::Infrastructure.CccListCollection<global::Domain.Agent>();
            agents.Add(agent);

            global::Domain.Group child1 = new global::Domain.Group(-1, "TestChild1", null, 1, agents);
            global::Domain.Group child2 = new global::Domain.Group(-1, "TestChild2", null, 2, agents);

            ICccListCollection<global::Domain.Group> children1 = new global::Infrastructure.CccListCollection<global::Domain.Group>();
            ICccListCollection<global::Domain.Group> children2 = new global::Infrastructure.CccListCollection<global::Domain.Group>();
            children1.Add(child1);
            children2.Add(child2);

            global::Domain.Group topGroup1 = new global::Domain.Group(1, "TopGroup1", children1, null, agents);
            global::Domain.Group topGroup2 = new global::Domain.Group(2, "TopGroup2", children2, null, agents);

            global::Domain.ICccListCollection<global::Domain.Group> groupCollection = new global::Infrastructure.CccListCollection<global::Domain.Group>();
            groupCollection.Add(topGroup1);
            groupCollection.Add(topGroup2);

            _oldEntity = new global::Domain.Grouping(-1, "TestGrouping", false, groupCollection);

            //setup new entity

            IChildPersonGroup childGroup1 = new ChildPersonGroup("TestChild1");           
            IChildPersonGroup childGroup2 = new ChildPersonGroup("TestChild2");
            
            IRootPersonGroup rootGroup1 = new RootPersonGroup("TopGroup1");
            rootGroup1.AddChildGroup(childGroup1);
            
            IRootPersonGroup rootGroup2 = new RootPersonGroup("TopGroup2");
            rootGroup2.AddChildGroup(childGroup2);

            IPerson person = mappedObjectPair.Agent.GetPaired(agent);
            if (person != null)
            {
                childGroup1.AddPerson(person);
                childGroup2.AddPerson(person);
                rootGroup1.AddPerson(person);
                rootGroup2.AddPerson(person);
            }

            _newEntity = new GroupPage("TestGrouping");
            _newEntity.AddRootPersonGroup(rootGroup1);
            _newEntity.AddRootPersonGroup(rootGroup2);

            //MappedObjectPair MappedPair = new MappedObjectPair();

            //setup GroupingMapper

            _groupingMapper = new GroupingMapper(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc));

        }

        /// <summary>
        /// Verifies the grouping name is mapped.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/5/2008
        /// </remarks>
        [Test]
        public void VerifyGroupingNameMapped()
        {
            IGroupPage gp = _groupingMapper.Map(_oldEntity);

            string expectedName = gp.Description.Name;
            string actualName = _newEntity.Description.Name;
            Assert.AreEqual(expectedName, actualName);
        }

        /// <summary>
        /// Verifies the top level groups are mapped.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/5/2008
        /// </remarks>
        [Test]
        public void VerifyTopLevelGroupsMapped()
        {
            IGroupPage gp = _groupingMapper.Map(_oldEntity);

            // for RootPersonGroup objects
            int expectedRootCount = gp.RootGroupCollection.Count;
            int actualRootCount = _newEntity.RootGroupCollection.Count;
            Assert.AreEqual(expectedRootCount, actualRootCount);

            for (int index = 0; index < actualRootCount; index++)
            {
                // tests name is mapped
                string expectedRootName = gp.RootGroupCollection[index].Description.Name;
                string actualRootName = _newEntity.RootGroupCollection[index].Description.Name;
                Assert.AreEqual(expectedRootName, actualRootName);

                // tests BuildPersonCollection()
                int expectedRootPersonCount = gp.RootGroupCollection[index].PersonCollection.Count;
                int actualRootPersonCount = _newEntity.RootGroupCollection[index].PersonCollection.Count;
                Assert.AreEqual(expectedRootPersonCount, actualRootPersonCount);

                // tests BuildChildHierarchy()
                int expectedChildCount = gp.RootGroupCollection[index].ChildGroupCollection.Count;
                int actualChildCount = _newEntity.RootGroupCollection[index].ChildGroupCollection.Count;
                Assert.AreEqual(expectedChildCount, actualChildCount);
            }
        }
    }
}
