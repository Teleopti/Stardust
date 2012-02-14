using System;
using NUnit.Framework;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class ExternalAgentStateTest
    {
        private IExternalAgentState _target;
        private DateTime _date;
        private DateTime _batchId;

        [SetUp]
        public void Setup()
        {
            _date = DateTime.UtcNow;
            _batchId = DateTime.UtcNow;
            _target = new ExternalAgentState("45", "67", TimeSpan.Zero, _date, Guid.Empty, 1, _batchId,false);
        }

        [Test]
        public void VerifyHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(),true));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("45", _target.ExternalLogOn);
            Assert.AreEqual("67", _target.StateCode);
            Assert.AreEqual(_date, _target.Timestamp);
            Assert.AreEqual(Guid.Empty, _target.PlatformTypeId);
            Assert.AreEqual(TimeSpan.Zero, _target.TimeInState);
            Assert.AreEqual(1, _target.DataSourceId);
            Assert.AreEqual(_batchId, _target.BatchId);
            Assert.AreEqual(false,_target.IsSnapshot);

            Guid platformId = Guid.NewGuid();

            _target.ExternalLogOn = "47";
            _target.StateCode = "69";
            _target.Timestamp = DateTime.SpecifyKind( _date.AddDays(1),DateTimeKind.Local);
            _target.PlatformTypeId = platformId;
            _target.TimeInState = TimeSpan.FromSeconds(14);
            _target.IsSnapshot = true;

            Assert.AreEqual("47", _target.ExternalLogOn);
            Assert.AreEqual("69", _target.StateCode);
            Assert.AreEqual(_date.AddDays(1), _target.Timestamp);
            Assert.AreEqual(_date.AddDays(1).Kind, _target.Timestamp.Kind);
            Assert.AreEqual(platformId, _target.PlatformTypeId);
            Assert.AreEqual(TimeSpan.FromSeconds(14), _target.TimeInState);
            Assert.AreEqual(true,_target.IsSnapshot);
        }
    }
}
