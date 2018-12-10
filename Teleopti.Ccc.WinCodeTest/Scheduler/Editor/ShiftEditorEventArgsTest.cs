using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Editor
{
    [TestFixture]
    public class ShiftEditorEventArgsTest
    {
        private ShiftEditorEventArgs _target;
        private IScheduleDay _part;
        private MockRepository _mocker;
        private ILayer<IActivity> _layer;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _part = _mocker.StrictMock<IScheduleDay>();
            _layer = _mocker.StrictMock<ILayer<IActivity>>();
            _period = new DateTimePeriod(2001,1,1,2001,1,2);
        }

        [Test]
        public void VerifyPart()
        {
            _target = new ShiftEditorEventArgs(_part);
            Assert.IsNull(_target.Period);
            Assert.IsNull(_target.SelectedLayer);
            Assert.AreEqual(_target.SchedulePart,_part);

        }

        [Test]
        public void VerifyPeriod()
        {
            _target = new ShiftEditorEventArgs(_part, _period);
            Assert.AreEqual(_target.Period, _period);
            Assert.IsNull(_target.SelectedLayer);
        }

        [Test]
        public void VerifyLayer()
        {
             _target = new ShiftEditorEventArgs(_part,_layer);
            Assert.AreEqual(_layer,_target.SelectedLayer);
            Assert.IsNull(_target.Period);
        }
        
    }
}
