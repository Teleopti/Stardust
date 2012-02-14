using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Models;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class VisualPayloadInfoTest
    {
        private VisualPayloadInfo _target;

        [SetUp]
        public void Setup()
        {
            _target = new VisualPayloadInfo(DateTime.MinValue, DateTime.MaxValue, Color.Blue, "Phone");
        }

        [Test]
        public void VerifyPropertiesCanRead()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(DateTime.MinValue, _target.StartTime);
            Assert.AreEqual(DateTime.MaxValue, _target.EndTime);
            Assert.AreEqual(Color.Blue, _target.Color);
            Assert.AreEqual("Phone", _target.Name);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfActivitiesIsNull()
        {
            _target.SetUnderlyingActivities(null);
        }

        [Test]
        public void ShouldHaveInfoAboutUnderlyingActivities()
        {
            Assert.That(_target.UnderlyingActivities,Is.EqualTo(""));
            var act = new Activity("activity");
            _target.SetUnderlyingActivities(new List<IActivity>{act});
            Assert.That(_target.UnderlyingActivities, Is.Not.EqualTo(""));
        }

        [Test]
        public void ShouldReturnTrueOnValidate()
        {
            Assert.That(_target.Validate(), Is.True);
        }
    }
}
