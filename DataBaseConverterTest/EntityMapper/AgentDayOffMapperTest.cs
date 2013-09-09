using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the AgentDayOffMapper
    /// </summary>
    [TestFixture]
    public class AgentDayOffMapperTest : MapperTest<global::Domain.AgentDay>
    {
        private AgentDayFactory _agdFactory;
        private global::Domain.AgentDay _agDay;
        private MappedObjectPair mappedObjectPair;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 13; }
        }

        /// <summary>
        /// Creates the agent day.
        /// </summary>
        [SetUp]
        public void CreateAgentDay()
        {
            _agdFactory = new AgentDayFactory();
            _agDay = _agdFactory.AgentDay();
            mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Agent = _agdFactory.AgentPairList;
            mappedObjectPair.Scenario = _agdFactory.ScenarioPairList;

                     
        }

        /// <summary>
        /// Determines whether this instance [can map agent base shift assignment].
        /// </summary>
        [Test]
        public void CanMapAgentAssignmentWithDayOffs()
        {
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.Absence("Day Off", "FR", true),
                new global::Domain.SchedType(-1, "Web", true, false, false));
            ObjectPairCollection<global::Domain.Absence, IDayOffTemplate> dayOffPairList = new ObjectPairCollection<global::Domain.Absence, IDayOffTemplate>();

            dayOffPairList.Add(_agDay.AgentDayAssignment.Assigned.AssignedAbsence, new DayOffTemplate(new Description("test")));
            mappedObjectPair.DayOff = dayOffPairList;

            AgentDayOffMapper agDayOffMapper = new AgentDayOffMapper(mappedObjectPair, (TimeZoneInfo.Utc));
            IPersonDayOff newPersonDayOff = agDayOffMapper.Map(_agDay);

            Assert.IsNotNull(newPersonDayOff.Person);
            Assert.IsNotNull(newPersonDayOff.Scenario);
        }
    }
}