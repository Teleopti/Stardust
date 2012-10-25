﻿using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class CreateLayerViewModelService : ICreateLayerViewModelService
    {

        private static ILayerViewModel CreateViewModelFromVisualLayer(IVisualLayer visualLayer, IEventAggregator eventAggregator, TimeSpan interval)
        {
            ILayerViewModel visualLayerViewModel;
            if (visualLayer.DefinitionSet != null) visualLayerViewModel = new OvertimeLayerViewModel(visualLayer, eventAggregator);
            else if (visualLayer.Payload is IAbsence) visualLayerViewModel = new AbsenceLayerViewModel(visualLayer, eventAggregator);
            else visualLayerViewModel = new MainShiftLayerViewModel(visualLayer, eventAggregator);
            ((LayerViewModel)visualLayerViewModel).IsProjectionLayer = true;

            visualLayerViewModel.Interval = interval;
            return visualLayerViewModel;
        }

        public IList<ILayerViewModel> CreateProjectionViewModelsFromSchedule(IScheduleRange scheduleRange, DateTimePeriod period, IEventAggregator eventAggregator, TimeSpan interval)
        {
            IList<ILayerViewModel> retList = new List<ILayerViewModel>();
            var timeZone = scheduleRange.Person.PermissionInformation.DefaultTimeZone();
            var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
			dateOnlyPeriod = new DateOnlyPeriod(dateOnlyPeriod.StartDate.AddDays(-1),dateOnlyPeriod.EndDate);
            foreach (DateOnly day in dateOnlyPeriod.DayCollection())
            {
                IScheduleDay scheduleDay = scheduleRange.ScheduledDay(day);
                if (scheduleDay.HasProjection)
                {
                    var projectedLayers = scheduleDay.ProjectionService().CreateProjection().FilterLayers(period);
                    foreach (IVisualLayer visualLayer in projectedLayers)
                    {
                        var viewModel = CreateViewModelFromVisualLayer(visualLayer, eventAggregator, interval);
                        viewModel.SchedulePart = scheduleDay;
                        retList.Add(viewModel);
                    }
                }
            }
            return retList;
        }

        public IList<ILayerViewModel> CreateProjectionViewModelsFromProjectionSource(IProjectionSource projectionSource, IEventAggregator eventAggregator, TimeSpan interval)
        {
            IList<ILayerViewModel> projectionViewModels = new List<ILayerViewModel>();
            if (projectionSource != null)
            {
                foreach (IVisualLayer visualLayer in projectionSource.ProjectionService().CreateProjection())
                {
                    projectionViewModels.Add(CreateViewModelFromVisualLayer(visualLayer, eventAggregator, interval));
                }
            }
            return projectionViewModels;
        }

        public virtual IList<ILayerViewModel> CreateViewModelsFromSchedule(IScheduleDay scheduleDay, IEventAggregator eventAggregator, TimeSpan interval, ILayerViewModelObserver observer)
        {
            InParameter.NotNull("scheduleDay", scheduleDay);
            IList<ILayerViewModel> layerViewModels = new List<ILayerViewModel>();
            IPersonAssignment assignment = scheduleDay.AssignmentHighZOrder();
            if (assignment != null)
            {
                if (assignment.MainShift != null)
                {
                    foreach (ILayer<IActivity> layer in assignment.MainShift.LayerCollection)
                    {
                        layerViewModels.Add(new MainShiftLayerViewModel(observer, layer, assignment.MainShift, eventAggregator));
                    }
                }

                foreach (IOvertimeShift overtimeShift in assignment.OvertimeShiftCollection)
                {
                    foreach (
                        IOvertimeShiftActivityLayer layer in overtimeShift.LayerCollectionWithDefinitionSet())
                    {
                        layerViewModels.Add(new OvertimeLayerViewModel(observer, layer, eventAggregator));
                    }
                }

                foreach (IPersonalShift shift in assignment.PersonalShiftCollection)
                {
                    foreach (ILayer<IActivity> layer in shift.LayerCollection)
                    {
                        layerViewModels.Add(new PersonalShiftLayerViewModel(observer, layer, shift, eventAggregator));
                    }
                }

            }
			// bug 14478 show meetings even if there is no assignment
			foreach (IPersonMeeting meeting in scheduleDay.PersonMeetingCollection())
            {
                layerViewModels.Add(new MeetingLayerViewModel(observer, meeting, eventAggregator));
            }
			
            foreach (IPersonAbsence persAbs in scheduleDay.PersonAbsenceCollection())
            {
                layerViewModels.Add(new AbsenceLayerViewModel(observer, persAbs.Layer, eventAggregator));
            }

            //Set interval and part....refact to ctor
            foreach (ILayerViewModel model in layerViewModels)
            {
                model.Interval = interval;
                model.SchedulePart = scheduleDay;
            }

            return layerViewModels;


        }

        public IList<ILayerViewModel> CreateViewModelsFromSchedule(ILayerViewModelSelector selector, IScheduleDay scheduleDay, IEventAggregator eventAggregator, TimeSpan interval, ILayerViewModelObserver observer)
        {
             IList<ILayerViewModel> ret = CreateViewModelsFromSchedule(scheduleDay,eventAggregator,interval,observer);
            
            if(selector.ScheduleAffectsSameDayAndPerson(scheduleDay))
            {
                selector.TryToSelectLayer(ret, observer);
            }

            return ret;
           
        }

    }
}
