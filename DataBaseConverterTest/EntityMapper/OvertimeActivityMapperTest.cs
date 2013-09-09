using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Activity=Domain.Activity;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for OvertimeActivityMapper
    /// </summary>
    [TestFixture]
    public class OvertimeActivityMapperTest : MapperTest<global::Domain.Overtime>
    {
        private OvertimeActivityMapper _target;
        private IDictionary<int, Activity> _activityDictionary;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            var overtimeActivity = new Activity(2, "Da overtime activity", Color.DeepPink, false, false, false, false,
                                                       false, false, false, false);
            var underlyingActivity = new Activity(3, "Da underlying activity", Color.DeepPink, false, false, false, false,
                                                       false, false, false, false);

            _activityDictionary = new Dictionary<int, Activity>();
            _activityDictionary.Add(2, overtimeActivity);
            _activityDictionary.Add(3, underlyingActivity);
            _target = new OvertimeActivityMapper(new MappedObjectPair(), _activityDictionary);
        }

        /// <summary>
        /// Determines whether this type [can validate number of properties].
        /// </summary>
        [Test]
        public void CanValidateNumberOfProperties()
        {
            Assert.AreEqual(11, PropertyCounter.CountProperties(typeof (Multiplicator)));
        }

        /// <summary>
        /// Determines whether this instance [can map overtime activity6x].
        /// </summary>
        [Test]
        public void CanMapOvertimeActivity6XAndShrinkName()
        {
            string oldName = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
            Color oldColor = Color.DeepPink;

            global::Domain.Overtime oldOvertime = new global::Domain.Overtime(2, oldName,
                                                                         oldColor, true, false, true, 3);
            oldOvertime.PaidTime = true;
            IMultiplicatorDefinitionSet newMultiplicatorDefinitionSet = _target.Map(oldOvertime);

            Assert.AreEqual(oldName.Substring(0, 50), newMultiplicatorDefinitionSet.Name);
            //color should be color
            Assert.AreEqual(MultiplicatorType.Overtime, newMultiplicatorDefinitionSet.MultiplicatorType);
            Assert.AreEqual(1,_activityDictionary.Count);
            Assert.AreEqual(1, _target.MappedObjectPair.OvertimeUnderlyingActivity.Obj1Collection().Count);
        }

        [Test]
        public void CanHandleDeleted()
        {
            global::Domain.Overtime oldOvertime = new global::Domain.Overtime(2, "q",
                                                                          Color.Beige, true, true, true, 3);
            IMultiplicatorDefinitionSet newMultiplicatorDefinitionSet = _target.Map(oldOvertime);
            Assert.IsTrue(((IDeleteTag)newMultiplicatorDefinitionSet).IsDeleted);
            Assert.AreEqual(1, _activityDictionary.Count);
            Assert.AreEqual(1, _target.MappedObjectPair.OvertimeUnderlyingActivity.Obj1Collection().Count);
        }

        [Test]
        public void CanHandleNonExistingOvertimeActivity()
        {
            global::Domain.Overtime oldOvertime = new global::Domain.Overtime(-1, "q",
                                                                          Color.Beige, true, true, true, 3);
            Assert.IsNull(_target.Map(oldOvertime));
            Assert.AreEqual(2, _activityDictionary.Count);
            Assert.AreEqual(0, _target.MappedObjectPair.OvertimeUnderlyingActivity.Obj1Collection().Count);
        }

        [Test]
        public void CanHandleNonExistingUnderlyingActivity()
        {
            global::Domain.Overtime oldOvertime = new global::Domain.Overtime(2, "q",
                                                                          Color.Beige, true, true, true, -1);
            Assert.IsNull(_target.Map(oldOvertime));
            Assert.AreEqual(2, _activityDictionary.Count);
            Assert.AreEqual(0, _target.MappedObjectPair.OvertimeUnderlyingActivity.Obj1Collection().Count);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 8; }
        }
    }
}