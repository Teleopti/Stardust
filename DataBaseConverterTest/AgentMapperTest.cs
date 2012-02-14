using System;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.AgentInfo;
using Agent=Domain.Agent;

namespace Teleopti.Ccc.DatabaseConverterTest
{
    /// <summary>
    /// Tests for AgentMapper
    /// </summary>
    [TestFixture]
    public class AgentMapperTest
    {
        private AgentMapper _simpleAgentMapper;
        private Agent _oldAgent;

        /// <summary>
        /// Sets the up.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            MappedObjectPair mapped = new MappedObjectPair();
            mapped.Team = new ObjectPairCollection<UnitSub, Team>();
            _simpleAgentMapper = new AgentMapper(mapped, TimeZoneInfo.Utc);
            //IList<global::Domain.AgentPeriod> periods = new List<global::Domain.AgentPeriod>();
            ICccListCollection<AgentPeriod> agPeriods = new CccListCollection<AgentPeriod>();
            DatePeriod datePeriod = new DatePeriod(new DateTime(2007, 01, 01));
            UnitSub oldTeam = new UnitSub(-1, "Test", -1, false, null);
            agPeriods.Add(
                new AgentPeriod(datePeriod, -1, null, null, oldTeam, null, null, "", null, null,
                                new DateTime(2007, 01, 01)));
            agPeriods.FinishReadingFromDatabase(CollectionType.Locked);
            _oldAgent = new Agent(-1, "Kalle", "Kula", "Kalle@Kula.nu", "", null, agPeriods, null);
        }

        /// <summary>
        /// Determines whether this type [can validate number of properties].
        /// </summary>
        [Test]
        public void CanValidateNumberOfProperties()
        {
            Assert.AreEqual(14, PropertyCounter.CountProperties(typeof (Domain.AgentInfo.Agent)));
        }

        /// <summary>
        /// Determines whether this instance [can map agent6x].
        /// </summary>
        [Test]
        public void CanMapAgent6XName()
        {
            Domain.AgentInfo.Agent newAgent = _simpleAgentMapper.Map(_oldAgent);
            //do not test dbid
            Assert.AreEqual("Kalle Kula", newAgent.Name.ToString());
        }
    }
}