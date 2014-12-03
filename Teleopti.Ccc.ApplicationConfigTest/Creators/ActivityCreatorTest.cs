﻿using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class ActivityCreatorTest
    {
        private ActivityCreator _target;

        [SetUp]
        public void Setup()
        {
            _target = new ActivityCreator();
        }

        [Test]
        public void VerifyCanCreateAbsence()
        {
            IActivity activity = _target.Create("name", new Description("name", "shortName"), Color.BlanchedAlmond, false, true);
            Assert.AreEqual("name", activity.Name);
            Assert.AreEqual(new Description("name", "shortName"), activity.Description);
            Assert.AreEqual(Color.BlanchedAlmond, activity.DisplayColor);
            Assert.AreEqual(false, activity.InReadyTime);
            Assert.AreEqual(true, activity.InContractTime);
        }
    }
}
