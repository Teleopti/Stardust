using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Interfaces.Domain;

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

        [Test]
        public void ShouldThrowIfActivitiesIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target.SetUnderlyingActivities(null));
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
