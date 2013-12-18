using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
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
            Assert.AreEqual(Color.Empty,_target.ConfidentialDisplayColor(null,DateOnly.Today));
            Assert.AreEqual(_name,_target.ConfidentialDescription(null,DateOnly.Today).Name);
            Assert.IsFalse(_target.InContractTime);
            Assert.IsFalse(_target.IsLogOutState);
            Assert.IsNull(_target.Tracker);

            _target.Name = "newName";
            _target.Available = false;
            _target.DefaultStateGroup = true;
            _target.IsLogOutState = true;

            Assert.AreEqual("newName",_target.Name);
            Assert.AreEqual("newName", _target.ConfidentialDescription(null,DateOnly.Today).Name);
            Assert.IsFalse(_target.Available);
            Assert.IsTrue(_target.DefaultStateGroup);
            Assert.IsTrue(_target.IsLogOutState);
        }

        [Test]
        public void VerifyAddState()
        {
            _target.AddState("state 1", "100", Guid.NewGuid());
            IRtaState state1 = _target.StateCollection[0];

            Assert.AreEqual(state1.StateGroup.Name, _name);
            Assert.AreEqual(1, _target.StateCollection.Count);
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void VerifySetInContractTimeNotImplemented()
        {
            _target.InContractTime = true;
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void VerifySetTrackerNotImplemented()
        {
            _target.Tracker = null;
        }

		[Test]
		public void ShouldCloneCorrectly()
		{
			_target.AddState("state 1", "100", Guid.NewGuid());
			_target.StateCollection[0].SetId(Guid.NewGuid());
			_target.SetId(Guid.NewGuid());

			var clone = _target.EntityClone();
			clone.Name.Should().Be.EqualTo(_target.Name);
			clone.Id.Should().Be.EqualTo(_target.Id);
			clone.StateCollection.Should().Not.Be.SameInstanceAs(_target.StateCollection);
			clone.StateCollection[0].Should().Be.EqualTo(_target.StateCollection[0]);
			clone.StateCollection[0].Should().Not.Be.SameInstanceAs(_target.StateCollection[0]);

			clone = (IRtaStateGroup) _target.Clone();
			clone.Name.Should().Be.EqualTo(_target.Name);
			clone.Id.Should().Be.EqualTo(_target.Id);
			clone.StateCollection.Should().Not.Be.SameInstanceAs(_target.StateCollection);
			clone.StateCollection[0].Should().Be.EqualTo(_target.StateCollection[0]);
			clone.StateCollection[0].Should().Not.Be.SameInstanceAs(_target.StateCollection[0]);

			clone = _target.NoneEntityClone();
			clone.Name.Should().Be.EqualTo(_target.Name);
			clone.Id.HasValue.Should().Be.False();
			clone.StateCollection.Should().Not.Be.SameInstanceAs(_target.StateCollection);
			clone.StateCollection[0].Should().Not.Be.EqualTo(_target.StateCollection[0]);
			clone.StateCollection[0].Id.HasValue.Should().Be.False();
		}

        [Test]
        public void VerifySetDeleted()
        {
            _target.SetDeleted();
            Assert.IsTrue(_target.IsDeleted);
        }
    }
}
