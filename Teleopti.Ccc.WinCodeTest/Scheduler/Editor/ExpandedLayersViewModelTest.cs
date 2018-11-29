using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Editor
{
    [TestFixture]
    public class ExpandedLayersViewModelTest
    {
        private ExpandedLayersViewModel _target;
        private LayerViewModelCollection _layers;
        private ILayerViewModel _layer1;
        private ILayerViewModel _layer2;
        private MockRepository _mocker;
        

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _layer1 = _mocker.StrictMock<ILayerViewModel>();
            _layer2 = _mocker.StrictMock<ILayerViewModel>();
			_layers = new LayerViewModelCollection(null, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission());
            _layers.Add(_layer1);
            _layers.Add(_layer2);
            _target = new ExpandedLayersViewModel(_layers);
        }

        [Test]
        public void VerifyProperties()
        {
            //Period
            DateTimePeriod period = new DateTimePeriod(2001,1,1,2001,1,2);
            _target.Period = period;
            Assert.AreEqual(_target.Period,period);

            //Layers
            Assert.AreEqual(_target.Layers,_layers);

            //Expanded
            Assert.AreEqual(_target.Expanded,1d,"The default expanded value is 1");
            _target.Expanded = 0.3d;
            Assert.AreEqual(_target.Expanded,0.3d);

            Assert.IsTrue(_target.HideProjectionInEditMode);
            _target.HideProjectionInEditMode = false;
            Assert.IsFalse(_target.HideProjectionInEditMode);

            _target.LayerHeight = 4;
            Assert.AreEqual(4,_target.LayerHeight);
        }

        [Test]
        public void VerifyEditCommand()
        {
            Assert.IsFalse(_target.EditMode,"False from start");
            
            TesterForCommandModels testerForCommandModels = new TesterForCommandModels();
            Assert.IsTrue(testerForCommandModels.CanExecute(_target.ToggleEditModeCommand));
            
            testerForCommandModels.ExecuteCommandModel(_target.ToggleEditModeCommand);
            Assert.IsTrue(_target.EditMode);
            testerForCommandModels.ExecuteCommandModel(_target.ToggleEditModeCommand);
            Assert.IsFalse(_target.EditMode);
        }

        [Test]
        public void VerifyVisibility()
        {
            PropertyChangedListener listener = new PropertyChangedListener();

            Assert.IsTrue(_target.HideProjectionInEditMode);
            Assert.AreEqual(_target.ShowLayers, Visibility.Collapsed);
            Assert.AreEqual(_target.ShowProjection, Visibility.Visible);

            listener.ListenTo(_target);
            _target.EditMode = true;
            Assert.AreEqual(_target.ShowProjection, Visibility.Collapsed);
            Assert.AreEqual(_target.ShowLayers, Visibility.Visible);
            Assert.IsTrue(listener.HasFired("ShowProjection"));
            Assert.IsTrue(listener.HasFired("ShowLayers"));
            _target.HideProjectionInEditMode = false;
            Assert.AreEqual(_target.ShowLayers,Visibility.Visible);
            Assert.AreEqual(_target.ShowProjection, Visibility.Visible);

        }

    }
}
