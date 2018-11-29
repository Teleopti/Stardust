using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Editor
{
    /// <summary>
    /// Interaction logic for WpfShiftEditor.xaml
    /// </summary>
    public partial class WpfShiftEditor : IWpfShiftEditor,IShiftEditorObserver
    {
        #region IShiftEditorEvent Members
        public event EventHandler<ShiftEditorEventArgs> CommitChanges;
        public event EventHandler<ShiftEditorEventArgs> ShiftUpdated;
        public event EventHandler<ShiftEditorEventArgs> SelectionChanged;
        public event EventHandler<ShiftEditorEventArgs> AddActivity;
        public event EventHandler<ShiftEditorEventArgs> AddOvertime;
        public event EventHandler<ShiftEditorEventArgs> AddAbsence;
        public event EventHandler<ShiftEditorEventArgs> AddPersonalShift;
        public event EventHandler<EventArgs> MeetingClicked;
        public event EventHandler<CustomEventArgs<IPersonMeeting>> EditMeeting;
        public event EventHandler<CustomEventArgs<IPersonMeeting>> DeleteMeeting;
        public event EventHandler<CustomEventArgs<IPersonMeeting>> RemoveParticipant;
        public event EventHandler<CustomEventArgs<IPersonMeeting>> CreateMeeting;
        public event EventHandler<EventArgs> Undo;
	    public event EventHandler<EventArgs> ShowLayers;
        #endregion
		
        private ShiftEditorViewModel _model;
        private GenericEvent<TriggerShiftEditorUpdate> _triggerShiftEditoEvent;
        private SubscriptionToken _subscriptionToken;
		
		public bool Enabled
		{
			get { return (bool)GetValue(EnabledProperty); }
			set { SetValue(EnabledProperty, value); }
		}

		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.Register("Enabled", typeof(bool), typeof(WpfShiftEditor), new PropertyMetadata(false));
		
        public WpfShiftEditor(IEventAggregator eventAggregator,ICreateLayerViewModelService createLayerViewModelService, bool showMeetingsInContextMenu)
        {
			_model = new ShiftEditorViewModel(eventAggregator, createLayerViewModelService, showMeetingsInContextMenu, new EditableShiftMapper());
            _model.AddObserver(this); //Because we are using events for now. We could add schedscreen (if implementing IShiftEditorObserver)here instead of events..
            DataContext = _model;
            setupforEventAggregator(eventAggregator);
            InitializeComponent();
        }

        private void setupforEventAggregator(IEventAggregator eventAggregator)
        {
            //Listen for changes in editor
            _triggerShiftEditoEvent = eventAggregator.GetEvent<GenericEvent<TriggerShiftEditorUpdate>>();
            _subscriptionToken = _triggerShiftEditoEvent.Subscribe(TriggerUpdate);
        }

        private void TriggerUpdate(EventParameters<TriggerShiftEditorUpdate> obj)
        {
	        Enabled = false;
			ShiftUpdated?.Invoke(this, new ShiftEditorEventArgs(_model.SchedulePart));
			commitChanges(_model.SchedulePart);
        }

        #region IShiftEditor Members

        public void LoadSchedulePart(IScheduleDay schedule)
        {
            _model.LoadSchedulePart(schedule);
	        Enabled = true;
        }

        public IScheduleDay SchedulePart => _model.SchedulePart;

	    public TimeSpan Interval
        {
            get { return _model.Interval; }
            set { _model.Interval = value; }
        }

        #endregion

        //Note: This triggers the events, there is really no need for eventing. If you want to listen to editorchanges, implement IShiftEditorObserver and add your class as listener
        #region eventdelegates
        public void AddObserver(IShiftEditorObserver observer)
        {
            _model.AddObserver(observer);
        }
        public void RemoveObserver(IShiftEditorObserver observer)
        {
            _model.RemoveObserver(observer);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void LayerChanged(object sender, RoutedEventArgs e)
        {
            EditorShiftUpdated(SchedulePart);
            //Update the projection
            _model.Layers.CreateProjectionViewModels(_model.SchedulePart);
            _model.ZoomToPeriod(_model.Layers.TotalDateTimePeriod());
        }

        //Delegates to IShiftEditorEvent
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void PreviewLayerSelected(object sender, RoutedEventArgs e)
        {
            ILayerViewModel currentLayer = e.OriginalSource as ILayerViewModel;
            _model.SelectLayer(currentLayer);
			SelectionChanged?.Invoke(this, new ShiftEditorEventArgs(SchedulePart));


			FrameworkElement uiElement = sender as FrameworkElement;
	        uiElement?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

	        if(Keyboard.GetKeyStates(Key.LeftShift)==KeyStates.Down||Keyboard.GetKeyStates(Key.RightShift)==KeyStates.Down)
            {
                SelectGroupMoveLayers(currentLayer);
            }
        }

        private void SelectGroupMoveLayers(ILayerViewModel selectedLayer)
        {
            if(selectedLayer!=null)
            _model.Layers.ForEach(o => o.CanMoveAll = o.ShouldBeIncludedInGroupMove(selectedLayer));
        }

        //Delegates to IShiftEditorEvent
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void PreviewMeetingSelected(object sender, RoutedEventArgs e)
        {
			MeetingClicked?.Invoke(this, EventArgs.Empty);
		}

        public void EditorShiftUpdated(IScheduleDay part)
        {
			ShiftUpdated?.Invoke(this, new ShiftEditorEventArgs(part));
		}

        public void EditorUpdateCommandExecuted(IScheduleDay part)
        {
			ShiftUpdated?.Invoke(this, new ShiftEditorEventArgs(SchedulePart));
		}

	    public void EditorAddActivity(IScheduleDay part, DateTimePeriod? period)
	    {
			var handler = AddActivity;
			if (handler != null)
			{
				ShiftEditorEventArgs shiftEditorEventArgs = GetShiftEditorEventArgs(part, period, null);
				handler(this, shiftEditorEventArgs);
			}
	    }

	    public void EditorSelectionChanged(IScheduleDay part)
        {
			SelectionChanged?.Invoke(this, new ShiftEditorEventArgs(part));
		}

        public void EditorAddActivity(IScheduleDay part, DateTimePeriod? period, IPayload payload)
        {
            var handler = AddActivity;
            if (handler != null)
            {
                ShiftEditorEventArgs shiftEditorEventArgs = GetShiftEditorEventArgs(part, period, payload);
                handler(this, shiftEditorEventArgs);
            }
        }

        private static ShiftEditorEventArgs GetShiftEditorEventArgs(IScheduleDay part, DateTimePeriod? period, IPayload payload)
        {
            ShiftEditorEventArgs shiftEditorEventArgs;
	        var activity = payload as IActivity;
	        ILayer<IPayload> selectedLayer = null;
	        if (activity != null)
		        selectedLayer = new ActivityLayer(activity, new DateTimePeriod());

            if (period.HasValue && period.Value.EndDateTime>DateTime.MinValue)
            {
                var periodWithoutSeconds = truncatePeriodSeconds(period.Value);
				shiftEditorEventArgs = new ShiftEditorEventArgs(part, periodWithoutSeconds, selectedLayer);
            }
            else
            {
				shiftEditorEventArgs = new ShiftEditorEventArgs(part, selectedLayer);
            }
            return shiftEditorEventArgs;
        }

        private static DateTimePeriod truncatePeriodSeconds(DateTimePeriod dateTimePeriod)
        {
            return
                new DateTimePeriod(
                    dateTimePeriod.StartDateTime.Date.AddMinutes(
                        (int)dateTimePeriod.StartDateTime.TimeOfDay.TotalMinutes),
                    dateTimePeriod.EndDateTime.Date.AddMinutes(
                        (int)dateTimePeriod.EndDateTime.TimeOfDay.TotalMinutes));
        }

        public void EditorAddOvertime(IScheduleDay part, DateTimePeriod? period)
        {
            var handler = AddOvertime;
            if (handler != null)
            {
                ShiftEditorEventArgs shiftEditorEventArgs = GetShiftEditorEventArgs(part, period, null);
                handler(this, shiftEditorEventArgs);
            }
        }

        public void EditorAddAbsence(IScheduleDay part, DateTimePeriod? period)
        {
            var handler = AddAbsence;
            if (handler != null)
            {
                ShiftEditorEventArgs shiftEditorEventArgs = GetShiftEditorEventArgs(part, period, null);
                handler(this, shiftEditorEventArgs);
            }
        }

        public void EditorAddPersonalShift(IScheduleDay part, DateTimePeriod? period)
        {
            var handler = AddPersonalShift;
            if (handler != null)
            {
                ShiftEditorEventArgs shiftEditorEventArgs = GetShiftEditorEventArgs(part, period, null);
                handler(this, shiftEditorEventArgs);
            }
        }

        public void EditorCommitChangesExecuted(IScheduleDay part)
        {
			//Added this call to Shift updated here, makes the save button lite after update is pressed. /Peter
			ShiftUpdated?.Invoke(this, new ShiftEditorEventArgs(part));
			//************

			commitChanges(part);
        }

        private void commitChanges(IScheduleDay part)
        {
			CommitChanges?.Invoke(this, new ShiftEditorEventArgs(part));
		}

        public void EditorEditMeetingExecuted(IPersonMeeting personMeeting)
        {
			EditMeeting?.Invoke(this, new CustomEventArgs<IPersonMeeting>(personMeeting));
		}

        public void EditorDeleteMeetingExecuted(IPersonMeeting personMeeting)
        {
			DeleteMeeting?.Invoke(this, new CustomEventArgs<IPersonMeeting>(personMeeting));
        }

        public void EditorRemoveParticipantsFromMeetingExecuted(IPersonMeeting personMeeting)
        {
			RemoveParticipant?.Invoke(this, new CustomEventArgs<IPersonMeeting>(personMeeting));
		}

        public void EditorCreateMeetingExecuted(IPersonMeeting personMeeting)
        {
			CreateMeeting?.Invoke(this, new CustomEventArgs<IPersonMeeting>(personMeeting));
		}

	    public void EditorShowLayersExecuted()
	    {
			ShowLayers?.Invoke(this, EventArgs.Empty);
		}
		
	    public void OnUndo(EventArgs e)
        {
			Undo?.Invoke(this, e);
		}

        #endregion

        public IList<IPayload> SelectablePayloads => _model.EditLayer.SelectablePayloads;

	    public IList<IShiftCategory> SelectableShiftCategories => _model.Settings.ShiftCategories;

	    public void LoadFromStateHolder(ICommonStateHolder commonStateHolder)
        {
			foreach (var absence in commonStateHolder.Absences)
			{
				if (!((IDeleteTag)absence).IsDeleted)
					SelectablePayloads.Add(absence);
			}
        	foreach (var activity in commonStateHolder.Activities)
        	{
        		if (!((IDeleteTag)activity).IsDeleted)
					SelectablePayloads.Add(activity);
        	}
            foreach (var shiftCategory in commonStateHolder.ShiftCategories)
            {
                if (!((IDeleteTag)shiftCategory).IsDeleted)
                    SelectableShiftCategories.Add(shiftCategory);
            }
        }

        public void EnableMoveAllLayers(bool move)
        {
            _model.Layers.ForEach(o => o.CanMoveAll = move);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                SelectGroupMoveLayers(_model.SelectedLayer);
            if ((e.Key == Key.Z && e.KeyboardDevice.Modifiers == ModifierKeys.Control) )
            {
                OnUndo(null);
                e.Handled = true;
            }
            if (e.Key == Key.Return)
            {
                e.Handled = true;
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                EnableMoveAllLayers(false);
        }

        public void Unload()
        {
            _model.RemoveObserver(this);
            _triggerShiftEditoEvent.Unsubscribe(_subscriptionToken);
            _triggerShiftEditoEvent.Unsubscribe(TriggerUpdate);
            _model = null;
        }

        public void SetTimeZone(TimeZoneInfo timeZoneInfo)
        {
            VisualTreeTimeZoneInfo.SetTimeZoneInfo(this, timeZoneInfo);
        }
    }
}
