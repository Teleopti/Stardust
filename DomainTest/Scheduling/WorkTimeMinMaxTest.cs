using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces;
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
        	var cat1 = new ShiftCategory("itt");
            var cat2 = new ShiftCategory("två");
        	var timeSpan = TimeSpan.FromHours(14);
        	var poss1 = new PossibleStartEndCategory {ShiftCategory = cat1, StartTime = timeSpan, EndTime = timeSpan.Add(TimeSpan.FromHours(8))};
        	var poss2 = new PossibleStartEndCategory {ShiftCategory = cat1, StartTime = timeSpan, EndTime = timeSpan.Add(TimeSpan.FromHours(8))};
        	var poss3 = new PossibleStartEndCategory {ShiftCategory = cat2, StartTime = timeSpan, EndTime = timeSpan.Add(TimeSpan.FromHours(8))};

            IWorkTimeMinMax other = new WorkTimeMinMax();
            other.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
            other.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(18));
            other.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
			other.PossibleStartEndCategories = new List<IPossibleStartEndCategory>{poss1, poss2 };
			

            _target.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(9), TimeSpan.FromHours(11));
            _target.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(17));
            _target.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(11));
			_target.PossibleStartEndCategories = new List<IPossibleStartEndCategory> { poss1, poss2, poss3 };

            IWorkTimeMinMax expected = new WorkTimeMinMax();
            expected.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(11));
            expected.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18));
            expected.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(11));

            _target = _target.Combine(other);
            Assert.AreEqual(expected.StartTimeLimitation, _target.StartTimeLimitation);
            Assert.AreEqual(expected.EndTimeLimitation, _target.EndTimeLimitation);
            Assert.AreEqual(expected.WorkTimeLimitation, _target.WorkTimeLimitation);
			Assert.That(_target.PossibleStartEndCategories.Count, Is.EqualTo(2));
			
        }
    }
}