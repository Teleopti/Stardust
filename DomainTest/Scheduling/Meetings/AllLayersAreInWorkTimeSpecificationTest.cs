using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class AllLayersAreInWorkTimeSpecificationTest
    {
        private MockRepository _mocks;
        private IVisualLayer _layer;
        private IAbsence _absence;
        private IActivity _activity;
        private IFilteredVisualLayerCollection _filtered;
        private List<IVisualLayer> _lst;
        private IPayload _otherPayload;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _layer = _mocks.StrictMock<IVisualLayer>();
            
            _absence = _mocks.StrictMock<IAbsence>();
            _activity = _mocks.StrictMock<IActivity>();
            _otherPayload = _mocks.StrictMock<IPayload>();
            _filtered = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            _lst = new List<IVisualLayer> { _layer };
        }

        [Test]
        public void ShouldReturnFalseWhenNotAbsenceAndNotActivity()
        {
            Expect.Call(_filtered.GetEnumerator()).Return(_lst.GetEnumerator());
            Expect.Call(_layer.Payload).Return(_otherPayload).Repeat.Twice();
            _mocks.ReplayAll();
            Assert.That(new AllLayersAreInWorkTimeSpecification().IsSatisfiedBy(_filtered), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseWhenNotInWorkTime()
        {
            Expect.Call(_filtered.GetEnumerator()).Return(_lst.GetEnumerator());

            Expect.Call(_layer.Payload).Return(_activity);
            Expect.Call(_activity.InWorkTime).Return(false);
            _mocks.ReplayAll();
            Assert.That(new AllLayersAreInWorkTimeSpecification().IsSatisfiedBy(_filtered), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueWhenInWorkTime()
        {
            Expect.Call(_filtered.GetEnumerator()).Return(_lst.GetEnumerator());

            Expect.Call(_layer.Payload).Return(_activity);
            Expect.Call(_activity.InWorkTime).Return(true);
            _mocks.ReplayAll();
            Assert.That(new AllLayersAreInWorkTimeSpecification().IsSatisfiedBy(_filtered), Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseWhenAbsenceNotInWorkTime()
        {
            Expect.Call(_filtered.GetEnumerator()).Return(_lst.GetEnumerator());

            Expect.Call(_layer.Payload).Return(_absence).Repeat.Twice();
            Expect.Call(_absence.InWorkTime).Return(false);
            _mocks.ReplayAll();
            Assert.That(new AllLayersAreInWorkTimeSpecification().IsSatisfiedBy(_filtered), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueWhenAbsenceInWorkTime()
        {
            Expect.Call(_filtered.GetEnumerator()).Return(_lst.GetEnumerator());

            Expect.Call(_layer.Payload).Return(_absence).Repeat.Twice();
            Expect.Call(_absence.InWorkTime).Return(true);
            _mocks.ReplayAll();
            Assert.That(new AllLayersAreInWorkTimeSpecification().IsSatisfiedBy(_filtered), Is.True);
            _mocks.VerifyAll();
        }
    }

    
}