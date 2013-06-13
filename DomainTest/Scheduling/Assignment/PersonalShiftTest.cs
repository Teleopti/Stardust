using System;
using System.Reflection;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    /// <summary>
    /// Tests for PersonalShift
    /// </summary>
    [TestFixture]
    public class PersonalShiftTest
    {
        private PersonalShift target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new PersonalShift();
        }

        /// <summary>
        /// Verifies the clone method.
        /// </summary>
        [Test]
        public void VerifyClone()
        {
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            //ActivityLayer actLay1 = new ActivityLayer(DummyActivity, period1);
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            //ActivityLayer actLay2 = new ActivityLayer(DummyActivity, period2);
            typeof (Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target,
                                                                                                     Guid.NewGuid());
            target.LayerCollection.Add(new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period1));
            target.LayerCollection.Add(new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period2));
            PersonalShift clone = (PersonalShift) target.Clone();
            Assert.AreNotSame(target, clone);
            Assert.AreNotSame(target.LayerCollection, clone.LayerCollection);
            Assert.AreSame(target.LayerCollection[0].Payload, clone.LayerCollection[0].Payload);
            Assert.AreSame(target.LayerCollection[1].Payload, clone.LayerCollection[1].Payload);
            Assert.AreEqual(target.LayerCollection[0].Period, clone.LayerCollection[0].Period);
            Assert.AreEqual(target.LayerCollection[1].Period, clone.LayerCollection[1].Period);
            //Assert.IsNull(clone.Id);
            Assert.AreEqual(target.Id, clone.Id);
        }

        [Test]
        public void VerifyICloneableEntity()
        {
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            //ActivityLayer actLay1 = new ActivityLayer(DummyActivity, period1);
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            //ActivityLayer actLay2 = new ActivityLayer(DummyActivity, period2);
            typeof(Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target,
                                                                                                     Guid.NewGuid());
            PersonalShiftActivityLayer layer;
            layer = new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period1);
            ((IEntity)layer).SetId(Guid.NewGuid());
            target.LayerCollection.Add(layer);

            layer = new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period2);
            ((IEntity)layer).SetId(Guid.NewGuid());
            target.LayerCollection.Add(layer);

            PersonalShift personalShift = (PersonalShift)target.EntityClone();
            Assert.AreEqual(target.Id, personalShift.Id);
            Assert.AreEqual(((IPersonalShiftActivityLayer)target.LayerCollection[0]).Id, ((IPersonalShiftActivityLayer)personalShift.LayerCollection[0]).Id);

            personalShift = (PersonalShift)target.NoneEntityClone();
            Assert.IsNull(personalShift.Id);
            Assert.IsNull(((IPersonalShiftActivityLayer)personalShift.LayerCollection[0]).Id);

        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotAddNothingButPersonalShiftActivityLayer()
        {
            target.LayerCollection.Add(new MainShiftActivityLayer(new Activity("fd"), new DateTimePeriod(2002, 1, 1, 2003, 1, 1)));
        }

        [Test]
        public void OrderIndexIsMinusOneIfNotConnectedToAssignment()
        {
            Assert.AreEqual(-1, target.OrderIndex);
        }
        [Test]
        public void OrderIndexIsNotMinusOneWhenConnectedToAssignment()
        {
            PersonAssignment personAssignment = new PersonAssignment(new Person(), new Scenario("dsds"), new DateOnly(2000,1,1));
            personAssignment.AddPersonalShift(target);
            
            Assert.AreNotEqual(-1, target.OrderIndex);
        }
    }
}