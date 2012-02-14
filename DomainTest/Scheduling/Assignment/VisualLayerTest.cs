﻿using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class VisualLayerTest
    {
        private VisualLayer target;
        private DateTimePeriod period;
        private Activity activity;
        private IVisualLayerFactory layerFactory;

        [SetUp]
        public void Setup()
        {
            layerFactory = new VisualLayerFactory();
            period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
            activity = new Activity("df");
            target = (VisualLayer)layerFactory.CreateShiftSetupLayer(activity, period);
        }

        [Test]
        public void VerifyPersonIsSentToDisplayMethods()
        {
            MockRepository mocks = new MockRepository();
            var act = mocks.StrictMock<IActivity>();
            target = (VisualLayer)layerFactory.CreateShiftSetupLayer(act, period);
            target.Person = new Person();
            Color c = Color.Red;
            Description d = new Description("sdfsdf");
            using(mocks.Record())
            {
                Expect.Call(act.ConfidentialDescription(target.Person))
                    .Return(d);
                Expect.Call(act.ConfidentialDisplayColor(target.Person))
                    .Return(c);
            }
            using(mocks.Playback())
            {
                Assert.AreEqual(c, target.DisplayColor());
                Assert.AreEqual(d, target.DisplayDescription());
            }
        }

        [Test]
        public void CanCreate()
        {
            Assert.IsNotNull(target);
            Assert.AreSame(activity, target.Payload);
            Assert.AreSame(activity, target.HighestPriorityActivity);
            Assert.AreEqual(period, target.Period);
            Assert.IsNull(target.HighestPriorityAbsence);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyUnderlyingActivityMustNotBeNull()
        {
            layerFactory.CreateShiftSetupLayer(null, period);
        }
    }
}
