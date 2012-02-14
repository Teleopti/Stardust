using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.DomainTest.FakeData;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the AgentAvailabilityMapper
    /// </summary>
    [TestFixture]
    public class AgentAvailabilityMapperTest : MapperTest<global::Domain.AgentDay>
    {
        private AgentDayFactory _agdFactory;
        private global::Domain.AgentDay _agDay;
        private MappedObjectPair mappedObjectPair;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 12; }
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
        /// Determines whether this instance [can map agent availability].
        /// </summary>
        [Test, Ignore("This must be rewritten")]
        public void CanMapAgentAvailabilities()
        {
            _agDay.Limitation = _agdFactory.AgentLimitation();

            //AgentAvailabilityMapper agAvailabilityMapper = new AgentAvailabilityMapper(mappedObjectPair, TimeZoneInfo.Utc, defaultAvailability);
            //PersonAvailability newPersonAvailability = agAvailabilityMapper.Map(_agDay);

            //Assert.IsNotNull(newPersonAvailability.Person);
            //Assert.IsNotNull(newPersonAvailability.Scenario);
        }
    }
}