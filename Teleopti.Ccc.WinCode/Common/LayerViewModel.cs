using System;
using System.Drawing;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		private DateTimePeriod _period;
		private TimeSpan _interval = TimeSpan.FromMinutes(15);
		private bool _isChanged;
		private readonly ILayerViewModelObserver _parentObservingCollection;
		private IScheduleDay _part;
		private bool _canMoveAll;
		private readonly IEventAggregator _eventAggregator;
		private bool _isSelected;
		private IPayload _payload;


		protected LayerViewModel(ILayerViewModelObserver observer, ILayer<IPayload> layer, IEventAggregator eventAggregator, bool isProjectionLayer)
		{
			_payload = layer.Payload;
			_parentObservingCollection = observer;
			_eventAggregator = eventAggregator;
			Layer = layer;
			_period = Layer.Period;
			MoveUpCommand = CommandModelFactory.CreateCommandModel(MoveUp, CanExecuteMoveUp, UserTexts.Resources.MoveUp, ShiftEditorRoutedCommands.MoveUp);
			MoveDownCommand = CommandModelFactory.CreateCommandModel(MoveDown, CanExecuteMoveDown, UserTexts.Resources.MoveDown, ShiftEditorRoutedCommands.MoveDown);
			DeleteCommand = CommandModelFactory.CreateCommandModel(DeleteLayer, CanDelete, UserTexts.Resources.Delete, ShiftEditorRoutedCommands.Delete);
			IsProjectionLayer = isProjectionLayer;
		}

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
					SendPropertyChanged(nameof(CanMoveAll));
				}
			}
		}

		public void UpdateModel()
		{
			Replace();
			IsChanged = false;
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value != _isSelected)
				{
					_isSelected = value;
					SendPropertyChanged(nameof(IsSelected));
				}

			}
		}

		public bool IsChanged
		{
			get { return _isChanged; }
			set
			{
				if (_isChanged != value)
				{
					_isChanged = value;
					if (_isChanged)
					{
						ParentObservingCollection.ShouldBeUpdated(this);
					}
					SendPropertyChanged(nameof(IsChanged));
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

		protected ILayer<IPayload> Layer { get; private set; }

		public DateTimePeriod Period
		{
			get { return _period; }
			set
			{
				if (_period != value)
				{
					_period = value;
					SendPropertyChanged(nameof(Period));
					SendPropertyChanged(nameof(ElapsedTime));
				}
			}
		}

		public Color DisplayColor
		{
			get
			{
				var vs = Layer as IVisualLayer;
				return vs?.DisplayColor() ?? Layer.Payload.ConfidentialDisplayColor(SchedulePart.Person);
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
				var vs = Layer as IVisualLayer;
				return vs == null ? Layer.Payload.ConfidentialDescription(SchedulePart.Person).Name : vs.DisplayDescription().Name;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is  created from projection.
		/// </summary>
		public bool IsProjectionLayer
		{
			get;
			private set;
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


		public bool CanDelete()
		{
			return IsMovePermitted();
		}

		protected virtual void DeleteLayer()
		{
			//for activity layers - overriden in absencelayerviewmodel
			if (ParentObservingCollection != null)
			{
				ParentObservingCollection.RemoveActivity(this, (ShiftLayer)Layer, SchedulePart);
				new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
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

		//and payload....
		public virtual void UpdatePeriod()
		{
			if (IsChanged)
			{
				if (ParentObservingCollection != null)
				{
					ParentObservingCollection.UpdateAllMovedLayers();
				}
				if (_part != null)
				{
					new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
				}
				
				IsChanged = false;
			}
		}

		protected virtual void Replace()
		{
		}


		public abstract int VisualOrderIndex { get; }

		public void Delete()
		{
			if (CanDelete())
			{
				DeleteLayer();
			}
		}

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

		public void TimeChanged(FrameworkElement panel, double change)
		{
			if (IsMovePermitted())
			{
				var current = Period.StartDateTime;
				var currentElapsedtime = Period.ElapsedTime();
				TimeSpan t = DateTimePeriodPanel.GetTimeSpanFromHorizontalChange(panel, change * 0.6, _interval.TotalMinutes);
				var newStart = Period.StartDateTime.Add(t).ToInterval(Interval);
				var newEnd = newStart.Add(currentElapsedtime);
				Period = new DateTimePeriod(newStart, newEnd);
				IsChanged = true;

				TimeSpan delta = Period.StartDateTime.Subtract(current);
				MoveLayer(delta);
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
			IsChanged = true;
			if (ParentObservingCollection != null)
			{
				ParentObservingCollection.MoveAllLayers(this, span);
			}
		}

		public abstract bool CanMoveUp { get; }
		public abstract bool CanMoveDown { get; }


		public virtual IPayload Payload
		{
			get
			{
				return _payload;
			}
			set
			{
				if (IsPayloadChangePermitted)
				{
					_payload = value;
					IsChanged = true;
					SendPropertyChanged(nameof(Description));
					SendPropertyChanged(nameof(Payload));
					SendPropertyChanged(nameof(DisplayColor));
				}
			}
		}
	}
}
