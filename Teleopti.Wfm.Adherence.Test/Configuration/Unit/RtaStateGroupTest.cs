using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Unit
{
    [TestFixture]
    public class RtaStateGroupTest
    {
        private RtaStateGroup _target;
        private const string _name = "namn";
        private const bool _available = true;
        private const bool _defaultStateGroup = false;

        [SetUp]
        public void Setup()
        {
            _target = new RtaStateGroup(_name, _defaultStateGroup, _available);
        }

        [Test]
        public void CanCreateAndPropertiesWork()
        {
            Assert.AreEqual(_name, _target.Name);
            Assert.AreEqual(_available, _target.Available);
            Assert.AreEqual(_defaultStateGroup, _target.DefaultStateGroup);

            _target.Name = "newName";
            _target.Available = false;
            _target.DefaultStateGroup = true;

            Assert.AreEqual("newName",_target.Name);
            Assert.IsFalse(_target.Available);
            Assert.IsTrue(_target.DefaultStateGroup);
        }

        [Test]
        public void VerifyAddState()
        {
            _target.AddState("100", "state 1");
            IRtaState state1 = _target.StateCollection[0];

            Assert.AreEqual(state1.StateGroup.Name, _name);
            Assert.AreEqual(1, _target.StateCollection.Count);
        }
    }
}
