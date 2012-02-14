using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TimeLayer
{
    [TestFixture]
    public class MultiplicatorTest
    {
        private IMultiplicator _target;

        [SetUp]
        public void Setup()
        {
            _target = new Multiplicator(MultiplicatorType.Overtime);
            _target.Description = new Description("Hej", "HJ");
            _target.DisplayColor = Color.DarkViolet;
            _target.ExportCode = "b33";
            _target.MultiplicatorValue = 2.1;
        }

        [Test]
        public void VerifyConstructorAndProperties()
        {
            Assert.AreEqual(MultiplicatorType.Overtime, _target.MultiplicatorType);
            Assert.AreEqual("Hej", _target.Description.Name);
            Assert.AreEqual(Color.DarkViolet, _target.DisplayColor);
            Assert.AreEqual("b33", _target.ExportCode);
            Assert.AreEqual(2.1d, _target.MultiplicatorValue);
            _target.MultiplicatorType = MultiplicatorType.OBTime;
            Assert.AreEqual(MultiplicatorType.OBTime, _target.MultiplicatorType);
        }

        [Test]
        public void ShouldDelete()
        {
            var target = _target as Multiplicator;
            target.SetDeleted();
            target.IsDeleted.Should().Be.True();
        }

        [Test]
        public void ShouldClone()
        {
            _target.SetId(Guid.NewGuid());

            var clone = _target.EntityClone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.Id.Should().Be.EqualTo(_target.Id);

            clone = _target.NoneEntityClone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.Id.HasValue.Should().Be.False();

            clone = (IMultiplicator) _target.Clone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.Id.Should().Be.EqualTo(_target.Id);
        }
    }
}
