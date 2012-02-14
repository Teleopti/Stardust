using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the AgentAbsenceMapper
    /// </summary>
    [TestFixture]
    public class AgentAbsenceMapperTest : MapperTest<global::Domain.AgentDay>
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
            mappedObjectPair.Absence = _agdFactory.AbsPairList;
            mappedObjectPair.Agent = _agdFactory.AgentPairList;
            mappedObjectPair.Scenario = _agdFactory.ScenarioPairList;
        }

        /// <summary>
        /// Determines whether this instance [can map agent base shift assignment].
        /// </summary>
        [Test]
        public void CanMapAgentAssignmentWithAbsences()
        {
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.Absence("Semester", "SE", false),
                                                  new global::Domain.SchedType(-1, "Web", true, false, false));

            AgentAbsenceMapper agAbsMapper = new AgentAbsenceMapper(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc));
            IPersonAbsence newAgAbs = agAbsMapper.Map(_agDay);

            Assert.IsNotNull(newAgAbs.Person);
            Assert.IsNotNull(newAgAbs.Scenario);
        }
    }
}