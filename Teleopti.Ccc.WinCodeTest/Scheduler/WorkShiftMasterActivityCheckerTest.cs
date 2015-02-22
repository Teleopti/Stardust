using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class WorkShiftMasterActivityCheckerTest
    {
        private MockRepository _mocks;
        private IWorkShift _workShift;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _workShift = _mocks.StrictMock<IWorkShift>();
        }

        [Test]
        public void ShouldReturnTrueIfWorkShiftContainsMasterActivity()
        {
            var layer = _mocks.StrictMock<ILayer<IActivity>>();
            var payload = _mocks.StrictMock<IMasterActivity>();
            var layerColl = new LayerCollection<IActivity> {layer};

            Expect.Call(_workShift.LayerCollection).Return(layerColl);
            Expect.Call(layer.Payload).Return(payload);
            _mocks.ReplayAll();
            Assert.That(WorkShiftContainsMasterActivitySpecification.DoesContainMasterActivity(_workShift),Is.True);
            //_mocks.VerifyAll();

        }

        [Test]
        public void ShouldReturnFalseIfWorkShiftDoesNotContainMasterActivity()
        {
            var layer = _mocks.StrictMock<ILayer<IActivity>>();
            var payload = _mocks.StrictMock<IActivity>();
            var layerColl = new LayerCollection<IActivity> { layer };

            Expect.Call(_workShift.LayerCollection).Return(layerColl);
            Expect.Call(layer.Payload).Return(payload);
            _mocks.ReplayAll();
            Assert.That(WorkShiftContainsMasterActivitySpecification.DoesContainMasterActivity(_workShift), Is.False);
            //_mocks.VerifyAll();

        }

        [Test]
        public void ShouldReturnFalseIfWorkShiftIsNull()
        {
            _workShift = null;
            Assert.That(WorkShiftContainsMasterActivitySpecification.DoesContainMasterActivity(_workShift), Is.False);
            
        }
    }

    

}