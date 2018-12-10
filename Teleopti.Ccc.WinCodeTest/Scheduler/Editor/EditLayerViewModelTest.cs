using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Collections;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Editor
{
    [TestFixture]
    public class EditLayerViewModelTest
    {
        private EditLayerViewModel _target;
        private ILayerViewModel _layerViewModel;
        private MainShiftLayer _layer;
        private IActivity _activity;
        private DateTimePeriod _period;
        private TesterForCommandModels _commandTester;
     
        [SetUp]
        public void Setup()
        {
            _commandTester = new TesterForCommandModels();
            _activity = new Activity("test");
            _period = new DateTimePeriod(2001,1,1,2001,1,2);
            _layer = new MainShiftLayer(_activity, _period);
            _layerViewModel = new ModelForTest(MockRepository.GenerateMock<ILayerViewModelObserver>(), _layer);
            _target = new EditLayerViewModel();
        }

        [Test]
        public void VerifyIsEnabledWorks()
        {
            Assert.IsFalse(_target.IsEnabled);
            _target.Layer = _layerViewModel;
            Assert.IsTrue(_target.IsEnabled);
        }

        [Test]
        public void VerifyCanLoadLayerViewModel()
        {
            _target.Layer = _layerViewModel;
            Assert.AreEqual(_target.Layer,_layerViewModel);
            Assert.AreEqual(_target.Period.DateTimePeriod.StartDateTime,_layer.Period.StartDateTime);
            Assert.AreEqual(_target.Period.DateTimePeriod.EndDateTime, _layer.Period.EndDateTime);

            _target.Layer = null; //Just make sure it can be set back to null without any exceptions
        }


        [Test]
        public void VerifyUpdateLayerCommandCanExecute()
        {

            Assert.IsFalse(_commandTester.CanExecute(_target.UpdateLayerCommand), "Should not be able to update if layer isnt set");
            _target.Layer = _layerViewModel;
            Assert.IsTrue(_commandTester.CanExecute(_target.UpdateLayerCommand));

            ((ModelForTest) _layerViewModel).MovePermitted = false;
            Assert.IsFalse(_commandTester.CanExecute(_target.UpdateLayerCommand), "cannot Update if move isnt permitted");
        }

        [Test]
        public void VerifyUpdateLayerCommandExecute()
        {
            TesterForCommandModels models = new TesterForCommandModels();
            _target.Period.Start = _target.Period.Start.AddHours(1);
            _target.Layer = _layerViewModel;
            models.ExecuteCommandModel(_target.UpdateLayerCommand);
            Assert.AreEqual(_target.Period.DateTimePeriod, _layerViewModel.Period);

        }

        [Test]
        public void VerifyChangingThePeriodOfTheLayerUpdatesThePeriod()
        {
            //Note: This could be done with binding instead.
            DateTimePeriod newPeriod = new DateTimePeriod(2001,1,12,2001,1,14);
            _target.Layer = _layerViewModel;
            _layerViewModel.Period = newPeriod;
            Assert.AreEqual(newPeriod,_layerViewModel.Period);
        }

        [Test]
        public void VerifyOnlyShowsPayloadsOfCorrectType()
        {  
            //By sending in a LayerViewModel, the filter should be set to the type of the Payload:
            ShowOnlyCollection<IPayload> payloads = (ShowOnlyCollection<IPayload>) _target.SelectablePayloads;
            Assert.AreEqual(0,payloads.Filters.Count);
            _target.Layer = _layerViewModel;
            Assert.AreEqual(1, payloads.Filters.Count);
            Assert.AreEqual(payloads.Filters[0],_layerViewModel.Payload.GetType());
        }

        [Test]
        public void VerifyCorrectPayloadIsSelectedByDefaultIfItExists()
        {
            IActivity anotherActivity = new Activity("another Activity");
            _target.SelectablePayloads.Add(_activity);
            _target.SelectablePayloads.Add(anotherActivity);

            _layerViewModel.Payload = anotherActivity;
            _target.Layer = _layerViewModel;
            
            //Check the view
            IPayload selected = CollectionViewSource.GetDefaultView(_target.SelectablePayloads).CurrentItem as IPayload;
            Assert.AreEqual(anotherActivity, selected);
        }

        [Test]
        public void VerifyLayerPayloadChangesWhenUserSelectsNewPayloadAndPressesUpdate()
        {
            IActivity anotherActivity = new Activity("another Activity");
            _target.SelectablePayloads.Add(_activity);
            _target.SelectablePayloads.Add(anotherActivity);
            _target.Layer = _layerViewModel;

            //select new activity:
            CollectionViewSource.GetDefaultView(_target.SelectablePayloads).MoveCurrentTo(anotherActivity);
            TesterForCommandModels models = new TesterForCommandModels();
            models.ExecuteCommandModel(_target.UpdateLayerCommand);
            Assert.AreEqual(_layerViewModel.Payload,anotherActivity);

        }

        [Test]
        public void VerifyEventWhenLayerViewModelIsAltered()
        {
            ILayerViewModel model = null;
            _target.Layer = _layerViewModel;
            _target.LayerUpdated += ((sender, args) => model = args.Value);
           
            _commandTester.ExecuteCommandModel(_target.UpdateLayerCommand);
            Assert.AreEqual(model,_layerViewModel);
        }

        [Test]
        public void VerifyChangePayloadCommandCanExecuteOnlyIfPayloadIsDifferentFromCurrent()
        {
            IActivity activity1 = ActivityFactory.CreateActivity("another activity");
            _target.Layer = _layerViewModel;
            _target.SelectablePayloads.Add(activity1);
            _target.SelectablePayloads.Add(_activity);

            CollectionViewSource.GetDefaultView(_target.SelectablePayloads).MoveCurrentTo(_activity);
            Assert.IsFalse(_commandTester.CanExecute(_target.ChangePayloadCommand),"Cannot change activity if it is the same as the layer");

            CollectionViewSource.GetDefaultView(_target.SelectablePayloads).MoveCurrentToPosition(-1);
            Assert.IsFalse(_commandTester.CanExecute(_target.ChangePayloadCommand), "Cannot change activity the activity isnt included");

           CollectionViewSource.GetDefaultView(_target.SelectablePayloads).MoveCurrentTo(activity1);
           Assert.IsTrue(_commandTester.CanExecute(_target.ChangePayloadCommand));
        }

        [Test]
        public void VerifyChangePayloadCommandSetsThePayloadOfTheLayer()
        {
            IActivity activity = ActivityFactory.CreateActivity("new activity");
            _target.Layer = _layerViewModel;
            _target.SelectablePayloads.Add(activity);

            CollectionViewSource.GetDefaultView(_target.SelectablePayloads).MoveCurrentTo(activity);
            _commandTester.ExecuteCommandModel(_target.ChangePayloadCommand);

            Assert.AreEqual(_target.Layer.Payload,activity);
        }

        [Test]
        public  void VerifyUpdateLayerCommandProperties()
        {
            Assert.AreEqual(_target.UpdateLayerCommand.Text,UserTexts.Resources.Update);
        }

        

        private class ModelForTest : MainShiftLayerViewModel
        {
            public ModelForTest(ILayerViewModelObserver observer, MainShiftLayer layer) : base(observer,layer , new PersonAssignment(new Person(), new Scenario(), DateOnly.Today),new EventAggregator())
            {
                MovePermitted = true;
            }

         
            public override bool IsMovePermitted()
            {
                return MovePermitted;
            }

            public bool MovePermitted
            {
                get; set;
            }
        }
    }
}
