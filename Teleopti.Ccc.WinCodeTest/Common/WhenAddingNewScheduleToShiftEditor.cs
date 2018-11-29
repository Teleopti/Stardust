using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class WhenAddingNewScheduleToShiftEditor
    {
        private MockRepository _mocker;
        private IScheduleDay _scheduleDay;
        private IEventAggregator _eventAggregator;
        private ILayerViewModelObserver _observer;
        private ShiftEditorViewModel _shifteditorViewModel;
        private ICreateLayerViewModelService _service;
        private LayerViewModelCollection _collection;
	    private IEditableShiftMapper _editableShiftMapper;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _observer = _mocker.StrictMock<ILayerViewModelObserver>();
            _scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();
            _eventAggregator = new EventAggregator();
            _service = _mocker.StrictMock<ICreateLayerViewModelService>();
			_collection = new LayerViewModelCollection(_eventAggregator, _service, new RemoveLayerFromSchedule(), null, new FullPermission());
	        _editableShiftMapper = _mocker.StrictMock<IEditableShiftMapper>();
            _shifteditorViewModel = new ShiftEditorViewModel(_collection, _eventAggregator, _service, true, _editableShiftMapper);
        }

        [Test, Apartment(ApartmentState.STA)]
        public void VerifyDoesNotTryToSelectALayerIfNoLayerIsSelected()
        {
           
            using(_mocker.Record())
            {
                Expect.Call(_service.CreateViewModelsFromSchedule(_scheduleDay, _eventAggregator, TimeSpan.FromMinutes(15), _observer, new FullPermission()))
                    .Return(new List<ILayerViewModel>())
                    .IgnoreArguments();
            }
            
            using(_mocker.Playback())
            {
                _shifteditorViewModel.LoadSchedulePart(_scheduleDay);   
            }
        }
		
        [Test, Apartment(ApartmentState.STA)]
        public void VerifyThatSelectedLayerStillSelectedByCallingWithASelectorIfALayerIsSelected()
        {
            var mainShiftActivityLayer = new MainShiftLayer(ActivityFactory.CreateActivity("dummy"), new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
						ILayerViewModel layer = new MainShiftLayerViewModel(null, mainShiftActivityLayer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), null);
            _shifteditorViewModel.SelectedLayer = layer;

            using (_mocker.Record())
            {
                //when having a selected layer
                Expect.Call(_service.CreateViewModelsFromSchedule(null,_scheduleDay, _eventAggregator, TimeSpan.FromMinutes(15), _observer, new FullPermission()))
                    .Return(new List<ILayerViewModel>())
                    .IgnoreArguments();

            }

            using (_mocker.Playback())
            {
                _shifteditorViewModel.LoadSchedulePart(_scheduleDay);

            }
        }     
    }
}
