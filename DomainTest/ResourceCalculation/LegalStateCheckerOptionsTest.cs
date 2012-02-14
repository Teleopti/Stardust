using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class LegalStateCheckerOptionsTest
    {
        private LegalStateCheckerOptions _target;

        [Test]
        public void VerifyProperties()
        {
            _target = new LegalStateCheckerOptions();

            Assert.IsFalse(_target.UseRotations);
            _target.UseRotations = true;
            Assert.IsTrue(_target.UseRotations);

            Assert.IsFalse(_target.UseAvailability);
            _target.UseAvailability = true;
            Assert.IsTrue(_target.UseAvailability);

            Assert.IsFalse(_target.UseStudentAvailability);
            _target.UseStudentAvailability = true;
            Assert.IsTrue(_target.UseStudentAvailability);

            Assert.IsFalse(_target.UsePreferences);
            _target.UsePreferences = true;
            Assert.IsTrue(_target.UsePreferences);

            Assert.IsFalse(_target.UseSchedule);
            _target.UseSchedule = true;
            Assert.IsTrue(_target.UseSchedule);
        }
    }
}
