using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    /// <summary>
    /// Test clast for <see cref="SchedulePartTracker"/> class.
    /// </summary>
    [TestFixture]
    public class SchedulePartTrackerTest
    {
        private SchedulePartTracker _target;
        private IScheduleDay _originalPart;
        private IScheduleDay _changedPart;
        private MockRepository _mocks; 

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _originalPart = _mocks.StrictMock<IScheduleDay>();
            _changedPart = _mocks.StrictMock<IScheduleDay>();
            _target = new SchedulePartTracker(_originalPart, _changedPart);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_originalPart, _target.OriginalPart);
            Assert.AreEqual(_changedPart, _target.ChangedPart);
        }
    }
}
