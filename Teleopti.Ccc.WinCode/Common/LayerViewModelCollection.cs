﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Holds a collection of LayerViewModels that are created from a schedulepart or a projection
    /// </summary>
    public class LayerViewModelCollection : FilteredCollection<ILayerViewModel>, ILayerViewModelObserver
    {
        public LayerGroupViewModel AbsenceLayers { get; private set; }
        public LayerGroupViewModel MeetingLayers { get; private set; }
        public LayerGroupViewModel PersonalLayers { get; private set; }
        public LayerGroupViewModel OvertimeLayers { get; private set; }
        public LayerGroupViewModel MainShiftLayers { get; private set; }
        public ObservableCollection<LayerGroupViewModel> Groups
        {
            get { return _groups; }
        }
        
        private ICollectionView _visualLayers;
        private ICollectionView _scheduleLayers;

        private TimeSpan _interval = TimeSpan.FromMinutes(15); //Default
        private IScheduleDay _part;
        private IRemoveLayerFromScheduleService _removeService = new RemoveLayerFromScheduleService();
        private IEventAggregator _eventAggregator;
        private readonly ICreateLayerViewModelService _createLayerViewModelService;
        private ObservableCollection<LayerGroupViewModel> _groups = new ObservableCollection<LayerGroupViewModel>();

        public LayerViewModelCollection()
        {
        }
        public LayerViewModelCollection(IEventAggregator eventAggregator, ICreateLayerViewModelService createLayerViewModelService):this()
        {
            _eventAggregator = eventAggregator;
            _createLayerViewModelService = createLayerViewModelService;
            CreateGroups();
        }

        private void CreateGroups()
        {
            MainShiftLayers = new LayerGroupViewModel<MainShiftLayerViewModel>(UserTexts.Resources.Activities, this){OrderIndex = 0};
            PersonalLayers = new LayerGroupViewModel<PersonalShiftLayerViewModel>(UserTexts.Resources.PersonalShifts, this) { OrderIndex = 1 };
            OvertimeLayers = new LayerGroupViewModel<OvertimeLayerViewModel>(UserTexts.Resources.Overtime, this) { OrderIndex = 2 };
            MeetingLayers = new LayerGroupViewModel<MeetingLayerViewModel>(UserTexts.Resources.Meeting, this) { OrderIndex = 3 };
            AbsenceLayers = new LayerGroupViewModel<AbsenceLayerViewModel>(UserTexts.Resources.Absence, this) { OrderIndex = 4 };

            Groups.Add(MainShiftLayers);
            Groups.Add(PersonalLayers);
            Groups.Add(OvertimeLayers);
            Groups.Add(MeetingLayers);
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

        public IRemoveLayerFromScheduleService RemoveService
        {
            get { return _removeService; }
            set { _removeService = value; }
        }

        public void RemoveActivity(ILayerViewModel sender)
        {
            RemoveService.Remove(sender.SchedulePart, sender.Layer as ILayer<IActivity>);
            CreateProjectionViewModels(sender.SchedulePart);

            Remove(sender);
        }

        public void RemoveAbsence(ILayerViewModel sender)
        {
            RemoveService.Remove(sender.SchedulePart, sender.Layer as ILayer<IAbsence>);
            CreateProjectionViewModels(sender.SchedulePart);

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
            get { return _interval; }
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
                Add(CreateViewModelFromVisualLayer(visualLayer));
            }
        }

        public virtual void CreateViewModels(ILayerViewModelSelector selector, IScheduleDay scheduleDay)
        {
            if (scheduleDay == null) return;
            Clear();
            _part = scheduleDay;
            var layers = _createLayerViewModelService.CreateViewModelsFromSchedule(selector, scheduleDay, _eventAggregator, Interval, this);
            layers.ForEach(Add);

            foreach (IVisualLayer visualLayer in scheduleDay.ProjectionService().CreateProjection())
            {
                Add(CreateViewModelFromVisualLayer(visualLayer));
            }
        }

        public void CreateProjectionViewModels(IProjectionSource projectionSource)
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
                                                                                _eventAggregator, Interval).ForEach(Add);
        }

        public void AddFromSchedulePart(IScheduleDay scheduleDay)
        {
            if (scheduleDay == null) return;
            Clear();
            _part = scheduleDay;
            var layers = _createLayerViewModelService.CreateViewModelsFromSchedule(scheduleDay, _eventAggregator, Interval, this);
            layers.ForEach(Add);
        }

        public int IndexOfLayer(ILayer layer)
        {
            return IndexOf(this.FirstOrDefault(l => l.Layer == layer));
        }

        public DateTimePeriod TotalDateTimePeriod(bool includeAbsence)
        {
            IEnumerable<ILayerViewModel> listToCheck;

            if (includeAbsence)
            {
                listToCheck = this.Where(
                    v => v.Period.LocalStartDateTime.Date >= _part.Period.LocalStartDateTime.Date &&
                         v.Period.LocalEndDateTime.Date <= _part.Period.LocalEndDateTime.Date);
            }
            else
            {
                listToCheck = this.Where(v => !(v is AbsenceLayerViewModel));
            }
            if (listToCheck.Count() == 0) return _part.Period;

            return new DateTimePeriod(listToCheck.Min(l => l.Period.StartDateTime),
                                      listToCheck.Max(l => l.Period.EndDateTime));
        }

        public void MoveAllLayers(ILayerViewModel sender, TimeSpan timeSpanToMove)
        {
            foreach (ILayerViewModel model in this.Where(l => l.CanMoveAll))
            {
                if ((model.ShouldBeIncludedInGroupMove(sender)) && (model != sender)) model.Period = model.Period.MovePeriod(timeSpanToMove);
            }
        }

        public void LayerMovedVertically(ILayerViewModel sender)
        {
            CreateProjectionViewModels(_part);
        }

        private ILayerViewModel CreateViewModelFromVisualLayer(IVisualLayer visualLayer)
        {
            ILayerViewModel visualLayerViewModel;
            if (visualLayer.DefinitionSet != null) visualLayerViewModel = new OvertimeLayerViewModel(visualLayer);
            else if (visualLayer.Payload is IAbsence) visualLayerViewModel = AbsenceLayerViewModel.CreateForProjection(visualLayer);
            else visualLayerViewModel = new MainShiftLayerViewModel(visualLayer);
            ((LayerViewModel)visualLayerViewModel).IsProjectionLayer = true;

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

        /// <summary>
        /// Returns the current Layer (used for RTA)
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-25
        /// </remarks>
        public ILayer CurrentLayer(DateTime dateTime)
        {
            InParameter.VerifyDateIsUtc("dateTime", dateTime);
            var ret = this.FirstOrDefault((l => (l.IsProjectionLayer && l.Period.Contains(dateTime))));
            return ret != null ? ret.Layer : null;
        }

        /// <summary>
        /// Returns the next Layer (used for RTA)
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-25
        /// </remarks>
        public ILayer NextLayer(DateTime dateTime)
        {
            InParameter.VerifyDateIsUtc("dateTime", dateTime);
            var ret = this.FirstOrDefault(l => (l.IsProjectionLayer && l.Period.StartDateTime > dateTime));
            return ret != null ? ret.Layer : null;
        }
    }
}