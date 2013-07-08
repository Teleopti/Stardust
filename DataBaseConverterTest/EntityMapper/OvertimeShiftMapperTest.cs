using System;
using System.Linq;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the OvertimeShiftMapper
    /// </summary>
    [TestFixture]
    public class OvertimeShiftMapperTest : MapperTest<ShiftBase>
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
            var activity = agdFactory.ActPairList.Obj1Collection().First();
            FillupShift oldShift = agdFactory.FillUpShift();
            Overtime overtime = new Overtime(activity.Id,"Overtime", activity.ColorLayout,false,false,false,activity.Id);

            mappedObjectPair.OvertimeActivity.Add(activity,overtime);
            mappedObjectPair.OvertimeUnderlyingActivity.Add(activity,activity);
            mappedObjectPair.MultiplicatorDefinitionSet = new ObjectPairCollection<Overtime, IMultiplicatorDefinitionSet>();
            mappedObjectPair.MultiplicatorDefinitionSet.Add(overtime, new MultiplicatorDefinitionSet("test",MultiplicatorType.Overtime));

            OvertimeShiftMapper msMap = new OvertimeShiftMapper(mappedObjectPair, (TimeZoneInfo.Utc), new DateTime(2007, 1, 1));
            var newShift = msMap.Map(oldShift);

            Assert.AreEqual(2, newShift.Count());
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 3; }
        }
    }
}