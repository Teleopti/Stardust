using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class OvertimeShiftTest
    {
        private IOvertimeShift target;
        private IPerson owner;

        [SetUp]
        public void Setup()
        {
            target = new OvertimeShift();
            owner = new Person();
            IPersonAssignment ass = new PersonAssignment(owner, new Scenario("ff"), new DateOnly(2002, 1, 1));
            ass.AddOvertimeShift(target);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotAddNothingButPersonalShiftActivityLayer()
        {
            target.LayerCollection.Add(new WorkShiftActivityLayer(new Activity("fd"), new DateTimePeriod(2002, 1, 1, 2003, 1, 1)));
        }

        [Test]
        public void OrderIndexIsMinusOneIfNotConnectedToAssignment()
        {
            Assert.AreEqual(-1, new OvertimeShift().OrderIndex);
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void CannotAddLayerBeforeShiftIsConnectedToAssignment()
        {
            IOvertimeShift shift = new OvertimeShift();
            shift.LayerCollection.Add(new OvertimeShiftActivityLayer(new Activity("sdf"),
                                                                     new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
                                                                     new MultiplicatorDefinitionSet("f",MultiplicatorType.Overtime)));
        }

        [Test]
        public void VerifyCanReadAndWriteLayers()
        {
            IActivity act = new Activity("sdf");
            IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(owner, def);
            target.LayerCollection.Add(new OvertimeShiftActivityLayer(act, new DateTimePeriod(2001,1,1,2002,1,1), def));
            IEnumerable<IOvertimeShiftActivityLayer> coll = target.LayerCollectionWithDefinitionSet();

            Assert.AreEqual(1, coll.Count());
            Assert.AreEqual(new DateTimePeriod(2001,1,1,2002,1,1), coll.First().Period);
            Assert.AreSame(def, coll.First().DefinitionSet);
            Assert.AreSame(act, coll.First().Payload);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotAddLayerForIncorrectDefinitionSet()
        {
			PersonFactory.AddDefinitionSetToPerson(owner, new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime));
            IActivity act = new Activity("sdf");
            IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime);
            target.LayerCollection.Add(new OvertimeShiftActivityLayer(act, new DateTimePeriod(2001, 1, 1, 2002, 1, 1), def));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotAddLayerWhenNoDefinitionSetAreDefinedForPerson()
        {
            IActivity act = new Activity("sdf");
            IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime);
            target.LayerCollection.Add(new OvertimeShiftActivityLayer(act, new DateTimePeriod(2001, 1, 1, 2002, 1, 1), def));
        }
    }
}
