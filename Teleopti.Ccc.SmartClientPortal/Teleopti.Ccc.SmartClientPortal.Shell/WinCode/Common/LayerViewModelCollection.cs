using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Collections;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    /// <summary>
    /// Holds a collection of LayerViewModels that are created from a schedulepart or a projection
    /// </summary>
    public class LayerViewModelCollection : FilteredCollection<ILayerViewModel>, ILayerViewModelObserver
    {
        public LayerGroupViewModel AbsenceLayers { get; private set; }
        public LayerGroupViewModel MeetingLayers { get; private set; }
        public LayerGroupViewModel ExternalMeetingLayers { get; private set; }
        public LayerGroupViewModel PersonalLayers { get; private set; }
        public LayerGroupViewModel OvertimeLayers { get; private set; }
        public LayerGroupViewModel MainShiftLayers { get; private set; }
        public ObservableCollection<LayerGroupViewModel> Groups => _groups;

	    private ICollectionView _visualLayers;
        private ICollectionView _scheduleLayers;

        private TimeSpan _interval = TimeSpan.FromMinutes(15); //Default
        private IScheduleDay _schedule;
        private readonly IRemoveLayerFromSchedule _removeService;
	    private readonly IReplaceLayerInSchedule _replaceService;
		private readonly IAuthorization _authorization;
		private readonly IEventAggregator _eventAggregator;
        private readonly ICreateLayerViewModelService _createLayerViewModelService;
        private readonly ObservableCollection<LayerGroupViewModel> _groups = new ObservableCollection<LayerGroupViewModel>();
	    private readonly HashSet<ILayerViewModel> _layersThatShouldBeUpdated = new HashSet<ILayerViewModel>();
		
	    public LayerViewModelCollection(IEventAggregator eventAggregator, ICreateLayerViewModelService createLayerViewModelService, IRemoveLayerFromSchedule removeService, IReplaceLayerInSchedule replaceService, IAuthorization authorization)
        {
            _eventAggregator = eventAggregator;
            _createLayerViewModelService = createLayerViewModelService;
			_removeService = removeService;
			_replaceService = replaceService;
			_authorization = authorization;
			CreateGroups();
        }

        private void CreateGroups()
        {
            MainShiftLayers = new LayerGroupViewModel<MainShiftLayerViewModel>(UserTexts.Resources.Activities, this){OrderIndex = 0};
            PersonalLayers = new LayerGroupViewModel<PersonalShiftLayerViewModel>(UserTexts.Resources.PersonalShifts, this) { OrderIndex = 1 };
            OvertimeLayers = new LayerGroupViewModel<OvertimeLayerViewModel>(UserTexts.Resources.Overtime, this) { OrderIndex = 2 };
            MeetingLayers = new LayerGroupViewModel<MeetingLayerViewModel>(UserTexts.Resources.Meeting, this) { OrderIndex = 3 };
            ExternalMeetingLayers = new LayerGroupViewModel<ExternalMeetingLayerViewModel>(UserTexts.Resources.Meeting, this) { OrderIndex = 4 };
            AbsenceLayers = new LayerGroupViewModel<AbsenceLayerViewModel>(UserTexts.Resources.Absence, this) { OrderIndex = 5 };

            Groups.Add(MainShiftLayers);
            Groups.Add(PersonalLayers);
            Groups.Add(OvertimeLayers);
            Groups.Add(MeetingLayers);
            Groups.Add(ExternalMeetingLayers);
            Groups.Add(AbsenceLayers);

            CollectionViewSource.GetDefaultView(Groups).SortDescriptions.Add(new SortDescription("OrderIndex", ListSortDirection.Ascending));
        }

        /// <summary>
        /// Gets all the layers from the scheduleparts projection
        /// </summary>
        /// <value>The visual layers.</value>
        public ICollectionView VisualLayers
        {
            get
            {
                if (_visualLayers == null)
                {
                    CollectionViewSource viewSource = new CollectionViewSource { Source = this };
                    _visualLayers = viewSource.View;
                    _visualLayers.Filter = LayersFromProjection;
                }
                return _visualLayers;
            }
        }

        /// <summary>
        /// Gets all the layers in the schedulepart
        /// </summary>
        /// <value>The schedule layers.</value>
        public ICollectionView ScheduleLayers
        {
            get
            {
                if (_scheduleLayers == null)
                {
                    CollectionViewSource viewSource = new CollectionViewSource { Source = this };
                    _scheduleLayers = viewSource.View;
                    _scheduleLayers.Filter = LayersFromSchedulePart;
                    _scheduleLayers.SortDescriptions.Add(new SortDescription("VisualOrderIndex", ListSortDirection.Ascending));
                }
                return _scheduleLayers;
            }
        }


		public void RemoveActivity(ILayerViewModel sender, ShiftLayer activityLayer, IScheduleDay scheduleDay)
		{
			_removeService.Remove(scheduleDay, activityLayer);
			CreateProjectionViewModels(scheduleDay);

			Remove(sender);
		}

	    public void RemoveAbsence(ILayerViewModel sender, ILayer<IAbsence> absenceLayer, IScheduleDay scheduleDay)
	    {
			_removeService.Remove(scheduleDay, absenceLayer);
			CreateProjectionViewModels(scheduleDay);

			Remove(sender);
	    }

	    public void SelectLayer(ILayerViewModel model)
        {
            //deselect all other models:
            if (model != ScheduleLayers.CurrentItem || !model.IsSelected)
            {
                this.All(m => m.IsSelected = false);
                model.IsSelected = true;
                ScheduleLayers.MoveCurrentTo(model);
            }
        }

        /// <summary>
        /// Gets or sets the interval
        /// Sets the interval on all exisiting models in the collection and when creating new ones
        /// </summary>
        /// <value>The interval.</value>
        public TimeSpan Interval
        {
            get => _interval;
	        set
            {
                if (_interval != value)
                {
                    _interval = value;
                    foreach (var layer in this)
                    {
                        layer.Interval = Interval;
                    }
                }
            }
        }

        //This will be the only method for creating layers, then bind to the views
        //Override the default view if possible
        public virtual void CreateViewModels(IScheduleDay part)
        {
            AddFromSchedulePart(part);
            foreach (IVisualLayer visualLayer in part.ProjectionService().CreateProjection())
            {
                Add(CreateViewModelFromVisualLayer(visualLayer, part.Person));
            }
			_layersThatShouldBeUpdated.Clear();
        }

        public virtual void CreateViewModels(ILayerViewModelSelector selector, IScheduleDay scheduleDay)
        {
            if (scheduleDay == null) return;
            Clear();
            _schedule = scheduleDay;
            var layers = _createLayerViewModelService.CreateViewModelsFromSchedule(selector, scheduleDay, _eventAggregator, Interval, this, _authorization);
            layers.ForEach(Add);

            foreach (IVisualLayer visualLayer in scheduleDay.ProjectionService().CreateProjection())
            {
                Add(CreateViewModelFromVisualLayer(visualLayer, scheduleDay.Person));
            }
			_layersThatShouldBeUpdated.Clear();
        }

        public void CreateProjectionViewModels(IScheduleDay projectionSource)
        {
            IList<ILayerViewModel> projectionLayers =
                this.Where(l => l.IsProjectionLayer).ToList();
            foreach (var layer in projectionLayers)
            {
                Remove(layer);
            }

            _createLayerViewModelService.CreateProjectionViewModelsFromProjectionSource(projectionSource, Interval).ForEach(Add);
        }

        public void AddFromProjection(IScheduleRange range, DateTimePeriod period)
        {
            Clear();
            _createLayerViewModelService.CreateProjectionViewModelsFromSchedule(range, period,
                                                                                _eventAggregator, Interval, _authorization).ForEach(Add);
        }

        public void AddFromSchedulePart(IScheduleDay scheduleDay)
        {
            if (scheduleDay == null) return;
            Clear();
            _schedule = scheduleDay;
            var layers = _createLayerViewModelService.CreateViewModelsFromSchedule(scheduleDay, _eventAggregator, Interval, this, _authorization);
            layers.ForEach(Add);
        }

		public DateTimePeriod TotalDateTimePeriod()
		{
			if (this.IsEmpty()) return _schedule.Period;
			var periods = from l in this
						  select new
						  {
							  Start = l.Period.StartDateTime,
							  End = l.Period.EndDateTime,
							  IsAbsence = l is AbsenceLayerViewModel
						  };

			var periodsExcludeAbsence = periods.Where(p => !p.IsAbsence).ToList();
			if (periodsExcludeAbsence.IsEmpty()) return _schedule.Period;
			return new DateTimePeriod(periodsExcludeAbsence.Min(p => p.Start), periodsExcludeAbsence.Max(p => p.End));
		}

        public void MoveAllLayers(ILayerViewModel sender, TimeSpan timeSpanToMove)
        {
			ShouldBeUpdated(sender);
            foreach (ILayerViewModel model in this.Where(l => l.CanMoveAll))
            {
                if (model.ShouldBeIncludedInGroupMove(sender))
                {
					ShouldBeUpdated(model);
	                if (model != sender)
	                {
		                model.Period = model.Period.MovePeriod(timeSpanToMove);
	                }
                }
            }
        }

		public void ShouldBeUpdated(ILayerViewModel layerViewModel)
		{
				_layersThatShouldBeUpdated.Add(layerViewModel);
		}

        public void LayerMovedVertically(ILayerViewModel sender)
        {
            CreateProjectionViewModels(_schedule);
        }
		
	    private ILayerViewModel CreateViewModelFromVisualLayer(IVisualLayer visualLayer, IPerson person)
        {
            ILayerViewModel visualLayerViewModel;
            if (visualLayer.DefinitionSet != null) visualLayerViewModel = new OvertimeLayerViewModel(visualLayer, person);
            else if (visualLayer.Payload is IAbsence) visualLayerViewModel = new AbsenceLayerViewModel(visualLayer, person);
            else visualLayerViewModel = new MainShiftLayerViewModel(visualLayer, person);

            visualLayerViewModel.Interval = Interval;
            return visualLayerViewModel;
        }

        private static bool LayersFromSchedulePart(object layer)
        {
            return !LayersFromProjection(layer);
        }
		
        private static bool LayersFromProjection(object layer)
        {
            return ((ILayerViewModel)layer).IsProjectionLayer;
        }

	    public void ReplaceActivity(ILayerViewModel sender, ILayer<IActivity> theLayer, IScheduleDay part)
	    {
		    _replaceService?.Replace(part, theLayer, sender.Payload as IActivity, sender.Period);
	    }

	    public void ReplaceAbsence(ILayerViewModel sender, ILayer<IAbsence> layer, IScheduleDay scheduleDay)
	    {
		    _replaceService?.Replace(scheduleDay, layer, sender.Payload as IAbsence, sender.Period);
	    }

	    public void UpdateAllMovedLayers()
	    {
		    foreach (var layerViewModel in this.Intersect(_layersThatShouldBeUpdated))
		    {
			    layerViewModel.UpdateModel();
		    }
			_layersThatShouldBeUpdated.Clear();
	    }
    }
}