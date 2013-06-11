using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class AbsenceLayerViewModelTest
    {
        private readonly SchedulePartFactoryForDomain _factory = new SchedulePartFactoryForDomain();
        private readonly MockRepository _mocks = new MockRepository();
        private readonly TesterForCommandModels _models = new TesterForCommandModels();
        private readonly IEventAggregator _eventAggregator = new EventAggregator();
		private AbsenceLayerViewModel _target;
        private MockRepository mocks;
        private ILayer _layerWithPayload;
        private IPayload _payload;
        private IScheduleDay _scheduleDay;
        private CrossThreadTestRunner _testRunner;
        private PropertyChangedListener _listener;
    	private IPerson _person;
	    private const bool _movePermitted = true;
	    private TesterForCommandModels _testerForCommandModels;
	    private DateTimePeriod _period;

		protected virtual bool Opaque
		{
			get { return false; }
		}

		protected MockRepository Mocks
		{
			get { return mocks; }
		}
        [SetUp]
        public void Setup()
        {
            _listener = new PropertyChangedListener();
            _testerForCommandModels = new TesterForCommandModels();
            mocks = new MockRepository();
            _layerWithPayload = Mocks.StrictMock<ILayer<IPayload>>();
            _payload = ActivityFactory.CreateActivity("dfsdf");
            _scheduleDay = Mocks.StrictMock<IScheduleDay>();
        	_person = PersonFactory.CreatePerson();
            _period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 5, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 12, 6, 0, 0, 0, DateTimeKind.Utc));
            Expect.Call(_layerWithPayload.Payload).Return(_payload).Repeat.Any();
            Expect.Call(_layerWithPayload.Period).PropertyBehavior().Return(_period).IgnoreArguments().Repeat.Any();
            Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008,12,5), TimeZoneHelper.CurrentSessionTimeZone)).Repeat.Any();

            Mocks.ReplayAll();

			_layerWithPayload.Period = _period;
			_target =  new AbsenceLayerViewModel(_layerWithPayload, null);
            _testRunner = new CrossThreadTestRunner();
        }

      

        [Test]
        public void VerifyCorrectDescription()
        {
          
            Assert.AreEqual(UserTexts.Resources.Absence,_target.LayerDescription);
        }


        [Test]
        public void VerifyProperties()
        {
          
            _target.SchedulePart = _scheduleDay;

        	var payloadFromLayer = (IPayload) _layerWithPayload.Payload;

			Assert.AreEqual(payloadFromLayer.ConfidentialDisplayColor(_person, new DateOnly(2008, 12, 5)), _target.DisplayColor);
			Assert.AreEqual(payloadFromLayer.ConfidentialDescription(_person, new DateOnly(2008, 12, 5)).Name, _target.Description);
            Assert.AreEqual(_layerWithPayload.Period, _target.Period);
            Assert.AreEqual(TimeSpan.FromMinutes(15),_target.Interval);
            Assert.IsNull(_target.Parent);
            Assert.IsFalse(_target.IsChanged);
            Assert.AreEqual(_layerWithPayload, _target.Layer);
            Assert.AreSame(_scheduleDay, _target.SchedulePart);
            Assert.IsFalse(_target.CanMoveAll);
            Assert.AreEqual(Opaque,_target.Opaque);
            Mocks.BackToRecordAll();

            ILayer testLayer = Mocks.StrictMock<ILayer>();

            Mocks.ReplayAll();

            _target.SchedulePart = _scheduleDay;
            _target.Layer = testLayer;
            _target.Interval = TimeSpan.FromHours(1);

            Assert.AreEqual(_scheduleDay,_target.SchedulePart);
            Assert.AreEqual(testLayer,_target.Layer);
            Assert.AreEqual(TimeSpan.FromHours(1),_target.Interval);
        }

        [Test]
        public void VerifyCanMoveAll()
        {
          
            bool succeeded = false;
            _target.PropertyChanged += (x, y) =>
            {
                succeeded = true;
                Assert.AreEqual("CanMoveAll", y.PropertyName);
            };
            _target.CanMoveAll = true;
            Assert.IsTrue(succeeded);
        }

        [Test]
        public void VerifyIsSelected()
        {
           
            Assert.IsFalse(_target.IsSelected);

            _listener.ListenTo(_target);
            _target.IsSelected = true;
            Assert.IsTrue(_listener.HasFired("IsSelected"));
            Assert.IsTrue(_target.IsSelected);

        }

        [Test]
        public void VerifyCanSetIsChanged()
        {
          
            bool succeeded = false;
            _target.PropertyChanged += (x, y) =>
                                          {
                                              succeeded = true;
                                              Assert.AreEqual("IsChanged", y.PropertyName);
                                          };
            _target.IsChanged = true;
            Assert.IsTrue(succeeded);
        }

        [Test]
        public void VerifyCanSetPeriod()
        {
         
            Mocks.BackToRecord(_layerWithPayload);
            _layerWithPayload.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
            Mocks.ReplayAll();

            _listener.ListenTo(_target);
           
            _target.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));           
            Assert.IsTrue(_listener.HasFired("Period"));
        }

        [Test]
        public void VerifyUpdatePeriod()
        {
          
            Mocks.BackToRecord(_layerWithPayload);
            _layerWithPayload.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
            LastCall.Repeat.Twice();
            Mocks.ReplayAll();

            _target.IsChanged = true;
            _target.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
            _target.UpdatePeriod();
            Assert.IsFalse(_target.IsChanged);
        }

        [Test]
        public void VerifyStartTimeChangedWithSchedulePart()
        {
            _testRunner.RunInSTA(
                delegate
                    {
                       
                        DateTimePeriodPanel panel = GetPanel();
                        _target.SchedulePart = _scheduleDay;
                        _target.StartTimeChanged(panel, 1);
						Assert.AreEqual(_movePermitted, _target.IsChanged);
                    });
        }

        private DateTimePeriodPanel GetPanel()
        {
            DateTimePeriodPanel panel = new DateTimePeriodPanel();
            FieldInfo fieldSize = typeof(UIElement).GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            fieldSize.SetValue(panel, new System.Windows.Size(10, 2));
            DateTimePeriodPanel.SetClipPeriod(panel, false);
            DateTimePeriodPanel.SetDateTimePeriod(panel, _period);
            return panel;
        }

        [Test]
        public void VerifyEndTimeChangedWithSchedulePart()
        {
            _testRunner.RunInSTA(
                delegate
                    {
                      
                        DateTimePeriodPanel panel = GetPanel();
                        _target.SchedulePart = _scheduleDay;
                        _target.EndTimeChanged(panel, 1);
						Assert.AreEqual(_movePermitted, _target.IsChanged);
                    });
        }

        [Test]
        public void VerifyTimeChangedWithSchedulePart()
        {
            _testRunner.RunInSTA(
                delegate
                    {
                     
                        DateTimePeriodPanel panel = GetPanel();
                        _target.SchedulePart = _scheduleDay;
                        _target.TimeChanged(panel, 1);
						Assert.AreEqual(_movePermitted, _target.IsChanged);
                    });
        }

        [Test]
        public void VerifyStartTimeChangedWithoutSchedulePart()
        {
            _testRunner.RunInSTA(
                delegate
                    {
                       
                        DateTimePeriodPanel panel = GetPanel();
                        _target.StartTimeChanged(panel, 1);
						Assert.AreEqual(_movePermitted, _target.IsChanged);
                    });
        }

        [Test]
        public void VerifyEndTimeChangedWithoutSchedulePart()
        {
            _testRunner.RunInSTA(
                delegate
                    {
                      
                        DateTimePeriodPanel panel = GetPanel();
                        _target.EndTimeChanged(panel, 1);
						Assert.AreEqual(_movePermitted, _target.IsChanged);
                    });
        }

        [Test]
        public void VerifyTimeChangedWithoutSchedulePart()
        {
            _testRunner.RunInSTA(
                delegate
                    {
                       
                        DateTimePeriodPanel panel = GetPanel();
                        _target.TimeChanged(panel, 1);
						Assert.AreEqual(_movePermitted, _target.IsChanged);
                    });
        }

        [Test]
        public void VerifyMoveUpDownReturnsFalseIfParentCollectionIsNull()
       {
           
           Assert.IsFalse(_target.CanMoveUp);
           Assert.IsFalse(_target.CanMoveDown);
       }

        [Test]
        public void VerifyCanExecuteDeleteCommand()
        {
            Assert.AreEqual(_testerForCommandModels.CanExecute(_target.DeleteCommand), _target.IsMovePermitted());
            Assert.AreEqual(_target.DeleteCommand.Text,UserTexts.Resources.Delete);
        }

        [Test]
        public void VerifyPeriodElapsedTime()
        {
            PropertyChangedListener listener = new PropertyChangedListener().ListenTo(_target);
            _target.Period = _target.Period.MovePeriod(TimeSpan.FromHours(2));
            Assert.IsTrue(listener.HasFired("ElapsedTime"));
            Assert.AreEqual(_target.Period.ElapsedTime(),_target.ElapsedTime);

        }

        [TearDown]
        public void Teardown()
        {
            Mocks.VerifyAll();
        }
    
        [Test]
        public void VerifyCannotMoveAbsenceLayerVertical()
        {
            #region setup
            LayerViewModelCollection collection = new LayerViewModelCollection(_eventAggregator,new CreateLayerViewModelService());
            IScheduleDay part = _factory.CreateSchedulePartWithMainShiftAndAbsence();
            collection.AddFromSchedulePart(part);
            #endregion

            //Get the first absencelayer:
            ILayerViewModel model = collection.First(l => l is AbsenceLayerViewModel);
            Assert.IsFalse(model.CanMoveUp);
            Assert.IsFalse(model.CanMoveDown);
        }

        [Test]
        public void VerifyDeleteCommandCallsObserver()
        {
            #region setup
            ILayerViewModelObserver observer = _mocks.StrictMock<ILayerViewModelObserver>();
         
            IScheduleDay part = _factory.CreateSchedulePartWithMainShiftAndAbsence();
            var absenceLayer = part.PersonAbsenceCollection().First().Layer;
            AbsenceLayerViewModel model = new AbsenceLayerViewModel(observer, absenceLayer, _eventAggregator);

            #endregion
            #region expectations
            using (_mocks.Record())
            {
                Expect.Call(() => observer.RemoveAbsence(model));            
               
            }
            #endregion

            using (_mocks.Playback())
            {
               
                _models.ExecuteCommandModel(model.DeleteCommand);
            }
        }

    }
}
