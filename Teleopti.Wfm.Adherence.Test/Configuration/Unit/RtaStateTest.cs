using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Unit
{
    [TestFixture]
    public class RtaStateTest
    {
        private IRtaState _target;
        const string _name = "Logged on";
        const string _stateCode = "100";

        [Test]
        public void CanCreateAndPropertiesWork()
        {
            var stateGroup = new RtaStateGroup("StateGroup A", true, false);

            stateGroup.AddState(_stateCode, _name);
            _target = stateGroup.StateCollection[0];

            Assert.AreEqual(_name, _target.Name);
            Assert.AreEqual(_stateCode, _target.StateCode);
            Assert.AreEqual(stateGroup, _target.Parent);

            _target.Name = "Pause";
            _target.StateCode = "P001";

            Assert.AreEqual("Pause",_target.Name);
            Assert.AreEqual("P001",_target.StateCode);
        }

        [Test]
        public void VerifyCanChangeStateGroup()
        {
            var stateGroup1 = new RtaStateGroup("Pause", false, true);
            var stateGroup2 = new RtaStateGroup("ACD", true, false);
            stateGroup1.AddState(_stateCode, _name);
			_target = stateGroup1.MoveStateTo(stateGroup2, stateGroup1.StateCollection[0]);
			
            Assert.AreEqual(stateGroup2.Name, _target.Parent.Name);
            Assert.AreEqual(stateGroup2.Available, _target.Parent.Available);
            Assert.AreEqual(stateGroup2.DefaultStateGroup, _target.Parent.DefaultStateGroup);
            Assert.IsFalse(stateGroup1.StateCollection.Contains(_target));

            Assert.IsNull(stateGroup2.MoveStateTo(null, _target));

			Assert.AreEqual(stateGroup1.StateCollection.Count, 0);
			Assert.AreEqual(stateGroup2.StateCollection.Count, 0);

            Assert.IsNull(stateGroup2.MoveStateTo(stateGroup1, null));

            Assert.AreEqual(stateGroup1.StateCollection.Count, 0);
            Assert.AreEqual(stateGroup2.StateCollection.Count, 0);
        }
    }
}
