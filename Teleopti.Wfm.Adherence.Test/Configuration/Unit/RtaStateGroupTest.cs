using NUnit.Framework;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Unit
{
    [TestFixture]
    public class RtaStateGroupTest
    {
        private const string _name = "namn";
        private const bool _available = true;
        private const bool _defaultStateGroup = false;

        [Test]
        public void CanCreateAndPropertiesWork()
		{
			var target = new RtaStateGroup(_name, _defaultStateGroup, _available);
			Assert.AreEqual(_name, target.Name);
            Assert.AreEqual(_available, target.Available);
            Assert.AreEqual(_defaultStateGroup, target.DefaultStateGroup);

            target.Name = "newName";
            target.Available = false;
            target.DefaultStateGroup = true;

            Assert.AreEqual("newName",target.Name);
            Assert.IsFalse(target.Available);
            Assert.IsTrue(target.DefaultStateGroup);
        }

        [Test]
        public void VerifyAddState()
		{
			var target = new RtaStateGroup(_name, _defaultStateGroup, _available);
			target.AddState("100", "state 1");
            IRtaState state1 = target.StateCollection[0];

            Assert.AreEqual(state1.Parent.Name, _name);
            Assert.AreEqual(1, target.StateCollection.Count);
        }
    }
}
