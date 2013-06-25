using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the ActivityLayerMapper
    /// </summary>
    [TestFixture]
    public class ActivityLayerMapperTest : MapperTest<global::Domain.ActivityLayer>
    {
        private ActivityLayerMapper target;
        private DateTime usedDate;
        private MappedObjectPair mappedObjectPair;
        private global::Domain.Activity oldActivity;
        private Activity convertedActivity;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            oldActivity =
                new global::Domain.Activity(-1, "qwert", Color.AliceBlue, false, false, false, false, true, false, false, false);
            convertedActivity= ActivityFactory.CreateActivity("xyz", Color.AliceBlue);
            ObjectPairCollection<global::Domain.Activity, IActivity> pairList =
                new ObjectPairCollection<global::Domain.Activity, IActivity>();
            pairList.Add(oldActivity, convertedActivity);
            mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Activity = pairList;
            usedDate = new DateTime(2007, 1, 1);
            target = new ActivityLayerMapper(mappedObjectPair,
                                                ActivityLayerBelongsTo.PersonalShift,
                                                usedDate,
                                                (TimeZoneInfo.Utc));
        }

        /// <summary>
        /// Determines whether this instance [can map activity layer].
        /// </summary>
        [Test]
        public void CanMapActivityLayer()
        {
            global::Domain.ActivityLayer oldActLayer =
                new global::Domain.ActivityLayer(new global::Domain.TimePeriod(new TimeSpan(8, 9, 0), new TimeSpan(16, 17, 0)), oldActivity);

            var newActLayer = target.Map(oldActLayer);

            Assert.AreEqual(convertedActivity, newActLayer.Payload);
            Assert.AreEqual(new DateTime(2007, 1, 1, 8, 9, 0), newActLayer.Period.StartDateTime);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 2; }
        }

    }
}