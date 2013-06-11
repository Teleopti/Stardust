using System;
using System.Drawing;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Common.Models;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling.Editor;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Holds bindable properties for Layers
    /// </summary>
    public abstract class LayerViewModel : DataModel, ILayerViewModel
    {
        private ILayer _layer;
        private DateTimePeriod _period;
        private TimeSpan _interval = TimeSpan.FromMinutes(15);
        private bool _isChanged;
        private ILayerViewModelObserver _parentObservingCollection;
        private IScheduleDay _part;
        private readonly IShift _parent;
        private bool _canMoveAll;
        private IEventAggregator _eventAggregator;
        private bool _isSelected;


				protected LayerViewModel(ILayerViewModelObserver observer, ILayer layer, IShift parent, IEventAggregator eventAggregator)
				{
					_parentObservingCollection = observer;
					_eventAggregator = eventAggregator;
					_layer = layer;
					_period = _layer.Period;
					_parent = parent;
					MoveUpCommand = CommandModelFactory.CreateCommandModel(MoveUp, CanExecuteMoveUp, UserTexts.Resources.MoveUp, ShiftEditorRoutedCommands.MoveUp);
					MoveDownCommand = CommandModelFactory.CreateCommandModel(MoveDown, CanExecuteMoveDown, UserTexts.Resources.MoveDown, ShiftEditorRoutedCommands.MoveDown);
					DeleteCommand = CommandModelFactory.CreateCommandModel(DeleteLayer, CanDelete, UserTexts.Resources.Delete, ShiftEditorRoutedCommands.Delete);
				}


        #region properties

        protected ILayerViewModelObserver ParentObservingCollection
        {
            get { return _parentObservingCollection; }
        }

        protected IEventAggregator LocalEventAggregator
        {
            get { return _eventAggregator; }
        }

        public bool CanMoveAll
        {
            get { return _canMoveAll; }
            set
            {
                if (_canMoveAll != value)
                {
                    _canMoveAll = value;
                    SendPropertyChanged("CanMoveAll");
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    SendPropertyChanged("IsSelected");
                }

            }
        }

        public IShift Parent
        {
            get { return _parent; }
        }

        public bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                if (_isChanged != value)
                {
                    _isChanged = value;
                    SendPropertyChanged("IsChanged");
                }
            }
        }

        public IScheduleDay SchedulePart
        {
            get { return _part; }
            set { _part = value; }
        }

        public TimeSpan Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        public ILayer Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public DateTimePeriod Period
        {
            get { return _period; }
            set
            {
                if (_period != value)
                {
                    _period = value;
                    Layer.Period = _period;
                    SendPropertyChanged("Period");
                    SendPropertyChanged("ElapsedTime");
                }
            }
        }

        public Color DisplayColor
        {
            get
            {
                var vs = _layer as IVisualLayer;
				return vs == null ? ((IPayload)_layer.Payload).ConfidentialDisplayColor(SchedulePart.Person, SchedulePart.DateOnlyAsPeriod.DateOnly) : vs.DisplayColor();
            }
        }


        public TimeSpan ElapsedTime
        {
            get { return Period.ElapsedTime(); }
        }
        public string Description
        {
            get
            {
                var vs = _layer as IVisualLayer;
                return vs == null ? ((IPayload)_layer.Payload).ConfidentialDescription(SchedulePart.Person,SchedulePart.DateOnlyAsPeriod.DateOnly).Name : vs.DisplayDescription().Name;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is  created from projection.
        /// </summary>
        public bool IsProjectionLayer
        {
            get;
            set;
        }


        public virtual bool Opaque
        {
            get { return false; }
        }

        public CommandModel MoveUpCommand { get; protected set; }
        public CommandModel MoveDownCommand { get; protected set; }
        public CommandModel DeleteCommand { get; protected set; }

        //Descriptive text of the LayerType
        public abstract string LayerDescription { get; }


        #endregion

        public bool CanDelete()
        {
            return IsMovePermitted();
        }

        protected virtual void DeleteLayer()
        {
            //Handle where necessary.
            //This should probably be abstract
            if (ParentObservingCollection != null)
            {
                ParentObservingCollection.RemoveActivity(this);
                TriggerShiftEditorUpdate();
            }
        }

        public bool CanExecuteMoveDown()
        {
            return CanMoveDown;
        }

        public bool CanExecuteMoveUp()
        {
            return CanMoveUp;
        }

        public virtual void MoveDown()
        {
            //Override where necessary
        }

        public virtual void MoveUp()
        {
            //Override where necessary
        }

        public virtual bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
        {
            return sender != this && GetType() == sender.GetType() && !IsProjectionLayer;
        }

        public void TimeChanged(FrameworkElement parent, FrameworkElement panel, double horizontalChange)
        {
            if (IsMovePermitted())
            {
                var t = DateTimePeriodPanel.GetTimeSpanFromHorizontalChange(parent, panel, horizontalChange, _interval.TotalMinutes);
                calculateMove(t);
                MoveLayer(t);
                IsChanged = true;
            }
        }

        private void calculateMove(TimeSpan t)
        {
            var p = Period.MovePeriod(t);
            var start = p.StartDateTime;
            var end = p.EndDateTime;
            if (start == end) end = end.Add(Interval);
            if (TimeSpan.FromMinutes(start.Minute) == Interval) start.ToInterval((int) Interval.TotalMinutes);
            if (TimeSpan.FromMinutes(end.Minute) == Interval) end.ToInterval((int) Interval.TotalMinutes);
            Period = new DateTimePeriod(start, end);
        }
        
        public virtual void UpdatePeriod()
        {
            if (IsChanged)
            {
                Layer.Period = Period;
                if (_part != null)
                {
									hackToUpdateUnderlyingPersonAssignment();
                    //Trigger update ShiftEditor
                    new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
                }
                IsChanged = false;
            }
        }

        #region vertical sorting
        /// <summary>
        /// Gets the order index base, decides where in the collection different types should appear
        /// </summary>
        protected abstract int OrderIndexBase { get; }

        public virtual int VisualOrderIndex
        {
            get
            {
	            if (this is MeetingLayerViewModel)
		            return OrderIndexBase + 1;
	            return OrderIndexBase + Layer.OrderIndex;
            }
        }

        public void Delete()
        {
            if (CanDelete())
            {
                DeleteLayer();
            }
        }

        #endregion

        #region move

        public abstract bool IsMovePermitted();

        public virtual bool IsPayloadChangePermitted
        {
            get { return true; }
        }

        public void StartTimeChanged(FrameworkElement parent, double change)
        {
            if (IsMovePermitted())
            {
                TimeSpan t = DateTimePeriodPanel.GetTimeSpanFromHorizontalChange(parent, change, _interval.TotalMinutes);
                if (Period.StartDateTime.Add(t) <= Period.EndDateTime.Subtract(Interval))
                {
                    Period = new DateTimePeriod(Period.StartDateTime.Add(t).ToInterval(Interval),
                                                Period.EndDateTime);
                    IsChanged = true;
                }
            }
        }

        public void TimeChanged(FrameworkElement parent, double change)
        {
            if (IsMovePermitted())
            {
                TimeSpan t = DateTimePeriodPanel.GetTimeSpanFromHorizontalChange(parent, change, _interval.TotalMinutes);
                calculateMove(t);
                MoveLayer(t);
                IsChanged = true;
            }

        }
        public void EndTimeChanged(FrameworkElement parent, double change)
        {
            if (IsMovePermitted())
            {

                TimeSpan t = DateTimePeriodPanel.GetTimeSpanFromHorizontalChange(parent, change, _interval.TotalMinutes);
                if (Period.EndDateTime.Add(t) >= Period.StartDateTime.Add(Interval))
                {
                    Period = new DateTimePeriod(Period.StartDateTime, Period.EndDateTime.Add(t).ToInterval(Interval));
                    IsChanged = true;
                }
            }
        }

        public void MoveLayer(TimeSpan span)
        {
            if (ParentObservingCollection != null)
            {
                ParentObservingCollection.MoveAllLayers(this, span);
            }
        }

        protected void TriggerShiftEditorUpdate()
        {
	        new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
        }

	    private void hackToUpdateUnderlyingPersonAssignment()
	    {
		    var mainShiftLayer = Layer as IMainShiftActivityLayer;
		    if (mainShiftLayer != null)
		    {
			    var ms = (IMainShift) mainShiftLayer.Parent;
			    ((IPersonAssignment) ms.Parent).SetMainShift(ms);
		    }
	    }

	    public abstract bool CanMoveUp { get; }
        public abstract bool CanMoveDown { get; }

        #endregion

        #region ILayerViewModel Members


        public virtual IPayload Payload
        {
            get
            {
                return _layer.Payload as IPayload;
            }
            set
            {
                if (IsPayloadChangePermitted && value != _layer.Payload)
                {
                    _layer.Payload = value;
                    IsChanged = true;
                    SendPropertyChanged("Description");
                    SendPropertyChanged("Payload");
                    SendPropertyChanged("DisplayColor");
                }
            }
        }

        #endregion


    }
}
