using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the PersonalShiftMapper
    /// </summary>
    [TestFixture]
    public class PersonalShiftMapperTest : MapperTest<global::Domain.FillupShift>
    {
        private MappedObjectPair mappedObjectPair;
        private AgentDayFactory agdFactory;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            agdFactory = new AgentDayFactory();
            mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Activity = agdFactory.ActPairList;
        }

        /// <summary>
        /// Determines whether this instance [can map work fillupshift6x].
        /// </summary>
        [Test]
        public void CanMapFillUpShift6X()
        {
            global::Domain.FillupShift oldShift = agdFactory.FillUpShift();

            PersonalShiftMapper msMap = new PersonalShiftMapper(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc), new DateTime(2007, 1, 1));
            PersonalShift newShift = msMap.Map(oldShift);

            Assert.AreEqual(3, newShift.LayerCollection.Count);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 4; }
        }
    }
}