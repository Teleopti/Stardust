using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class WorkShiftVisualLayerInfoTest
    {
        private WorkShiftVisualLayerInfo _target;
        private IWorkShift _workShift;
        private IVisualLayerCollection _layerCollection;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _workShift = _mocks.StrictMock<IWorkShift>();
            _layerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            _target = new WorkShiftVisualLayerInfo(_workShift, _layerCollection);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(_target.WorkShift);
            Assert.IsNotNull(_target.VisualLayerCollection);
        }

    }
}