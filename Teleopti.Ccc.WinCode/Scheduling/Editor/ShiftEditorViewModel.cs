﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Common.Time.Timeline;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Editor
{
    public class ShiftEditorViewModel : IShiftEditor, IShiftEditorSettingsTarget ,INotifyPropertyChanged
    {
        #region constants
        private const double ExpandSize = 0.8d;
        #endregion

        #region properties
        public TimeSpan SurroundingTime { get; private set; }
        public TimelineControlViewModel Timeline { get; private set; }
        public LayerViewModelCollection Layers { get; private set; }
	    public bool ShowMeetingsInContextMenu { get; private set; }
	    public ExpandedLayersViewModel AllLayers { get; private set; }
        public EditLayerViewModel EditLayer { get; private set; }
        public IScheduleDay SchedulePart { get; private set; }
        public ShiftEditorSettings Settings { get; private set; }
        public ILayerViewModel SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                if (_selectedLayer != value)
                {
                    _selectedLayer = value;

					NotifyPropertyChanged("SelectedLayer");
                }
            }
        }
        public TimeSpan Interval
        {
            get { return Settings.Interval; }
            set { Settings.Interval = value; }
        }

        public ReadOnlyCollection<IShiftEditorObserver> Observers
        {
            get { return new ReadOnlyCollection<IShiftEditorObserver>(_observers); }
        }
        
        #region commands
        public CommandModel DeleteCommand { get; private set; }
        public CommandModel AddActivityCommand { get; private set; }
        public CommandModel AddOvertimeCommand { get; private set; }
        public CommandModel AddAbsenceCommand { get; private set; }
        public CommandModel AddPersonalShiftCommand { get; private set; }
        public CommandModel UpdateCommand { get; private set; }
        public CommandModel CommitChangesCommand { get; private set; }
        public CommandModel EditMeetingCommand { get; private set;} //Old??
        public CommandModel DeleteMeetingCommand { get; private set;}
        public CommandModel RemoveParticipantCommand { get; private set;}
        public CommandModel CreateMeetingCommand { get; private set;}
        public CommandModel MoveUpCommand { get; private set; }
        public CommandModel MoveDownCommand { get; private set; }


        public CommandModel AddActivityWithPeriodCommand { get; private set; }
        public CommandModel AddOvertimeWithPeriodCommand { get; private set; }
        public CommandModel AddAbsenceWithPeriodCommand { get; private set; }
        public CommandModel AddPersonalWithPeriodShiftCommand { get; private set; }

        #endregion

        private IList<IShiftEditorObserver> _observers = new List<IShiftEditorObserver>();
        private ILayerViewModel _selectedLayer;
        private IShiftCategory _category;

        #endregion

        private IEventAggregator _eventAggregator;
	    private readonly IEditableShiftMapper _editableShiftMapper;

	    #region ctor
		public ShiftEditorViewModel(LayerViewModelCollection layerCollection, IEventAggregator eventAggregator, ICreateLayerViewModelService createLayerViewModelService, bool showMeetingsInContextMenu, IEditableShiftMapper editableShiftMapper)
        {
            _eventAggregator = eventAggregator;
			_editableShiftMapper = editableShiftMapper;
			SurroundingTime = TimeSpan.FromHours(4);
            Layers = layerCollection;
			ShowMeetingsInContextMenu = showMeetingsInContextMenu;
			Timeline = new TimelineControlViewModel(eventAggregator,createLayerViewModelService);
            AllLayers = new ExpandedLayersViewModel(Layers) {Expanded = ExpandSize };
			EditLayer = new EditLayerViewModel();
            EditLayer.LayerUpdated += EditLayer_LayerUpdated;
            Settings=new ShiftEditorSettings(this);
            SetUpCommandModels();
        }


		public ShiftEditorViewModel(IEventAggregator eventAggregator, ICreateLayerViewModelService createLayerViewModelService, bool showMeetingsInContextMenu, IEditableShiftMapper editableShiftMapper)
			: this(new LayerViewModelCollection(eventAggregator, createLayerViewModelService, new RemoveLayerFromSchedule(), null), eventAggregator, createLayerViewModelService, showMeetingsInContextMenu, editableShiftMapper)
        {

        }
        #endregion

        #region methods
        void EditLayer_LayerUpdated(object sender, Domain.Common.CustomEventArgs<ILayerViewModel> e)
        {
            CommitChangesExecuted();
        }

        public void SelectLayer(ILayerViewModel model)
        {
            if (model != SelectedLayer) EditLayer.Layer = model;
            SelectedLayer = model;
        }

        public void AddObserver(IShiftEditorObserver observer)
        {
            if(!_observers.Contains(observer))_observers.Add(observer);
        }
        public void RemoveObserver(IShiftEditorObserver observer)
        {
            if (_observers.Contains(observer)) _observers.Remove(observer);
        }
        #region commandmethods
        
        void SetUpCommandModels()
        {
            DeleteCommand = CommandModelFactory.CreateCommandModel(DeleteLayer,CanDeleteLayer,ShiftEditorRoutedCommands.Delete);
            AddActivityCommand = CommandModelFactory.CreateCommandModel(AddActivityExecuted,ShiftEditorRoutedCommands.AddMainLayer);
            AddOvertimeCommand = CommandModelFactory.CreateCommandModel(AddOvertimeExecuted,ShiftEditorRoutedCommands.AddOvertime);
            AddAbsenceCommand = CommandModelFactory.CreateCommandModel(AddAbsenceExecuted,ShiftEditorRoutedCommands.AddAbsenceLayer);
            AddPersonalShiftCommand = CommandModelFactory.CreateCommandModel(AddPersonalShiftExecuted,ShiftEditorRoutedCommands.AddPersonalShift);

            //todo
            AddActivityWithPeriodCommand = CommandModelFactory.CreateCommandModel(AddActivityWithPeriodExecuted, ShiftEditorRoutedCommands.AddActivityWithPeriod);
            AddOvertimeWithPeriodCommand = CommandModelFactory.CreateCommandModel(AddOvertimeWithPeriodExecuted, ShiftEditorRoutedCommands.AddOvertimeWithPeriod);
            AddAbsenceWithPeriodCommand = CommandModelFactory.CreateCommandModel(AddAbsenceWithPeriodExecuted, ShiftEditorRoutedCommands.AddAbsenceWithPeriod);
            AddPersonalWithPeriodShiftCommand = CommandModelFactory.CreateCommandModel(AddPersonalShiftWithPeriodExecuted, ShiftEditorRoutedCommands.AddPersonalShiftWithPeriod);

            UpdateCommand = CommandModelFactory.CreateCommandModel(UpdateExecuted, ShiftEditorRoutedCommands.Update);
            CommitChangesCommand = CommandModelFactory.CreateCommandModel(CommitChangesExecuted, ShiftEditorRoutedCommands.CommitChanges);
            CreateMeetingCommand = CommandModelFactory.CreateApplicationCommandModel(CreateMeetingExecuted,ShiftEditorRoutedCommands.CreateMeeting,DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
            DeleteMeetingCommand = CommandModelFactory.CreateApplicationCommandModel(DeleteMeetingExecuted, VerifyCurrentLayerIsMeeting,ShiftEditorRoutedCommands.DeleteMeeting, DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
            RemoveParticipantCommand = CommandModelFactory.CreateApplicationCommandModel(RemoveParticipantExecuted, VerifyCurrentLayerIsMeeting,ShiftEditorRoutedCommands.RemoveParticipant, DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
            EditMeetingCommand = CommandModelFactory.CreateApplicationCommandModel(EditMeetingExecuted,VerifyCurrentLayerIsMeeting,ShiftEditorRoutedCommands.EditMeeting,DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
            MoveUpCommand = CommandModelFactory.CreateCommandModel(MoveUpLayerExecuted, CanMoveUpLayer,ShiftEditorRoutedCommands.MoveUp);
            MoveDownCommand = CommandModelFactory.CreateCommandModel(MoveDownLayerExecuted, CanMoveDownLayer,ShiftEditorRoutedCommands.MoveDown);
        
        }

       

        private void MoveUpLayerExecuted()
        {
            SelectedLayer.MoveUp();
        }

        private bool CanMoveUpLayer()
        {
            return SelectedLayer != null && SelectedLayer.CanMoveUp;
        }

        private void MoveDownLayerExecuted()
        {
            SelectedLayer.MoveDown();
        }

        private bool CanMoveDownLayer()
        {
          return SelectedLayer != null && SelectedLayer.CanMoveDown;
        }

        private void DeleteLayer()
        {
          //  Layers.Remove(SelectedLayer);
            SelectedLayer.Delete();
        }

        private bool CanDeleteLayer()
        {
            return SelectedLayer != null && SelectedLayer.CanDelete();
        }

        private bool VerifyCurrentLayerIsMeeting()
        {
            return GetCurrentPersonMeeting()!=null;
        }

        private IPersonMeeting GetCurrentPersonMeeting()
        {
            var meetingLayerViewModel = SelectedLayer as MeetingLayerViewModel;
            if (meetingLayerViewModel != null) return meetingLayerViewModel.PersonMeeting;
            return null;
        }

        private void EditMeetingExecuted()
        {
            _observers.ForEach(o => o.EditorEditMeetingExecuted(GetCurrentPersonMeeting()));
        }

        private void RemoveParticipantExecuted()
        {
            _observers.ForEach(o => o.EditorRemoveParticipantsFromMeetingExecuted(GetCurrentPersonMeeting()));  
        }

        private void DeleteMeetingExecuted()
        {
            _observers.ForEach(o => o.EditorDeleteMeetingExecuted(GetCurrentPersonMeeting()));
            
        }

        private void CreateMeetingExecuted()
        {
            _observers.ForEach(o => o.EditorCreateMeetingExecuted(null));
        }

        private void CommitChangesExecuted()
        {
            _observers.ForEach(o => o.EditorCommitChangesExecuted(SchedulePart));
        }


        private void UpdateExecuted()
        {
            _observers.ForEach(o => o.EditorUpdateCommandExecuted(SchedulePart));
            
        }

        private void AddPersonalShiftExecuted()
        {
            var period = new DateTimePeriod();
            if (SchedulePart != null)
                period = Scheduling.AddActivityCommand.GetDefaultPeriodFromPart(SchedulePart);
            _observers.ForEach(o => o.EditorAddPersonalShift(SchedulePart, period));
        }

        private void AddAbsenceExecuted()
        {
            _observers.ForEach(o => o.EditorAddAbsence(SchedulePart, null));
        }

        private void AddOvertimeExecuted()
        {
            _observers.ForEach(o => o.EditorAddOvertime(SchedulePart, null));
        }

        private void AddActivityExecuted()
        {
            var period = new DateTimePeriod();

            if(SchedulePart != null)
                period = Scheduling.AddActivityCommand.GetDefaultPeriodFromPart(SchedulePart);

            _observers.ForEach(o => o.EditorAddActivity(SchedulePart, period));
        }
        private void AddPersonalShiftWithPeriodExecuted()
        {
            _observers.ForEach(o => o.EditorAddPersonalShift(SchedulePart, Timeline.SelectedPeriod));
        }

        private void AddAbsenceWithPeriodExecuted()
        {
            _observers.ForEach(o => o.EditorAddAbsence(SchedulePart, Timeline.SelectedPeriod));
        }

        private void AddOvertimeWithPeriodExecuted()
        {
            _observers.ForEach(o => o.EditorAddOvertime(SchedulePart, Timeline.SelectedPeriod));
        }

        private void AddActivityWithPeriodExecuted()
        {
            _observers.ForEach(o => o.EditorAddActivity(SchedulePart, Timeline.SelectedPeriod));
        }

        #endregion
        #endregion

        public void LoadSchedulePart(IScheduleDay schedule)
        {

            Enabled = false;
            if (schedule != null)
            {
                SchedulePart = schedule;


                if (SelectedLayer != null) 
                    Layers.CreateViewModels(new LayerViewModelSelector(SelectedLayer), schedule); 
                else Layers.CreateViewModels(schedule);
                
                
                
                ZoomToPeriod(Layers.TotalDateTimePeriod(true));
                var assignement = SchedulePart.AssignmentHighZOrder();
				if (assignement != null && assignement.ShiftCategory != null)
                {
                    _category = assignement.ShiftCategory;
                    CollectionViewSource.GetDefaultView(Categories).MoveCurrentTo(_category);
                    Enabled = true;
                    NotifyPropertyChanged("Category"); 
                }
                
                //henrik 20100701 todo, change this to something that can handle in gui....
                SelectLayer(Layers.FirstOrDefault(l => l.IsSelected));
                
            }
        }

       
       
        public void ZoomToPeriod(DateTimePeriod period)
        {
            Timeline.Period =
                  new DateTimePeriod(period.StartDateTime.Subtract(SurroundingTime),
                      period.EndDateTime.Add(SurroundingTime));
            AllLayers.Period = Timeline.Period;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public void SettingsAltered(ShiftEditorSettings settings)
        {
            Layers.Interval = settings.Interval;
            Timeline.Interval = settings.Interval;
            if (EditLayer != null) EditLayer.Period.Interval = settings.Interval;
        }

        public IShiftCategory Category
        {
            get
            {
               
                return _category;
            }
            set
            {
                if (_category != null && _category.Equals(value))
                    return;

                _category = value;

                CollectionViewSource.GetDefaultView(Categories).MoveCurrentTo(_category);
                if (SchedulePart != null)
                {
                    var assignement = SchedulePart.AssignmentHighZOrder();
					if (assignement != null)
					{
						var editorShift = _editableShiftMapper.CreateEditorShift(assignement);
                        editorShift.ShiftCategory = _category;
						_editableShiftMapper.SetMainShiftLayers(assignement, editorShift);
                        new TriggerShiftEditorUpdate().PublishEvent("ShiftEditorViewModel", _eventAggregator); 
                    }
                }
            }
        }

        public ObservableCollection<IShiftCategory> Categories
        {
            get { return Settings.ShiftCategories; }
        }

        private bool _enabled;

        public bool Enabled
        {
            get {
                return _enabled;
            }
            private set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    NotifyPropertyChanged("Enabled");
                }
            }
        }

        private void NotifyPropertyChanged(string property)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(property));
			}
        }
    }
}


   