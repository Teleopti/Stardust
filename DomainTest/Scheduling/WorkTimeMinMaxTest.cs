using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class WorkTimeMinMaxTest
    {
        private IWorkTimeMinMax _target;

        [SetUp]
        public void Setup()
        {
            _target = new WorkTimeMinMax();
        }

        [Test]
        public void VerifyInitialValues()
        {
            Assert.IsFalse(_target.StartTimeLimitation.HasValue());
            Assert.IsFalse(_target.EndTimeLimitation.HasValue());
            Assert.IsFalse(_target.WorkTimeLimitation.HasValue());
        }

        [Test]
        public void VerifyCombineOnInitialValues()
        {
            IWorkTimeMinMax other = new WorkTimeMinMax();
            other.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
            other.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(18));
            other.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));

            _target = _target.Combine(other);
            Assert.AreEqual(other.StartTimeLimitation, _target.StartTimeLimitation);
            Assert.AreEqual(other.EndTimeLimitation, _target.EndTimeLimitation);
            Assert.AreEqual(other.WorkTimeLimitation, _target.WorkTimeLimitation);
        }

        [Test]
        public void VerifyCombineEmpty()
        {
            IWorkTimeMinMax other = new WorkTimeMinMax();
            _target = _target.Combine(other);
            Assert.AreEqual(other.StartTimeLimitation, _target.StartTimeLimitation);
            Assert.AreEqual(other.EndTimeLimitation, _target.EndTimeLimitation);
            Assert.AreEqual(other.WorkTimeLimitation, _target.WorkTimeLimitation);
        }

        [Test]
        public void VerifyCombine()
        {



            IWorkTimeMinMax other = new WorkTimeMinMax();
            other.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
            other.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(18));
            other.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));

			

            _target.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(11));
            _target.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(17));
            _target.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(11));


            IWorkTimeMinMax expected = new WorkTimeMinMax();
            expected.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(11));
            expected.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18));
            expected.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(11));

            _target = _target.Combine(other);
            Assert.AreEqual(expected.StartTimeLimitation, _target.StartTimeLimitation);
            Assert.AreEqual(expected.EndTimeLimitation, _target.EndTimeLimitation);
            Assert.AreEqual(expected.WorkTimeLimitation, _target.WorkTimeLimitation);
			
        }
    }
}