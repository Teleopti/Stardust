using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class TimeLayerOfDoubleTest
    {
        private TimeLayerOfDouble _target;
        private TimePeriod _period;

        [SetUp]
        public void Setup()
        {
            _period = new TimePeriod(TimeSpan.FromHours(1),TimeSpan.FromHours(3));
            _target = new TimeLayerOfDouble(7.5d, _period);
        }

        [Test]

        public void VerifyNoPublicEmptyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(TimeLayerOfDouble)));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_period,_target.Period);
            Assert.AreEqual(7.5d, _target.Payload);
        }
    }
}
