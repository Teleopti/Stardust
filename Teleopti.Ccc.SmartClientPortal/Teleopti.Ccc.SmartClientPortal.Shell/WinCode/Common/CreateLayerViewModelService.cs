﻿using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class CreateLayerViewModelService : ICreateLayerViewModelService
    {

        private static ILayerViewModel createViewModelFromVisualLayer(IVisualLayer visualLayer, TimeSpan interval)
        {
            ILayerViewModel visualLayerViewModel;
            if (visualLayer.DefinitionSet != null) visualLayerViewModel = new OvertimeLayerViewModel(visualLayer);
            else if (visualLayer.Payload is IAbsence) visualLayerViewModel = new AbsenceLayerViewModel(visualLayer);
            else visualLayerViewModel = new MainShiftLayerViewModel(visualLayer);

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
                if (scheduleDay.HasProjection())
                {
                    var projectedLayers = scheduleDay.ProjectionService().CreateProjection().FilterLayers(period);
                    foreach (IVisualLayer visualLayer in projectedLayers)
                    {
                        var viewModel = createViewModelFromVisualLayer(visualLayer, interval);
                        viewModel.SchedulePart = scheduleDay;
                        retList.Add(viewModel);
                    }
                }
            }
            return retList;
        }

        public IList<ILayerViewModel> CreateProjectionViewModelsFromProjectionSource(IProjectionSource projectionSource, TimeSpan interval)
        {
            IList<ILayerViewModel> projectionViewModels = new List<ILayerViewModel>();
            if (projectionSource != null)
            {
                foreach (IVisualLayer visualLayer in projectionSource.ProjectionService().CreateProjection())
                {
                    projectionViewModels.Add(createViewModelFromVisualLayer(visualLayer, interval));
                }
            }
            return projectionViewModels;
        }

        public virtual IList<ILayerViewModel> CreateViewModelsFromSchedule(IScheduleDay scheduleDay, IEventAggregator eventAggregator, TimeSpan interval, ILayerViewModelObserver observer)
        {
            InParameter.NotNull("scheduleDay", scheduleDay);
            IList<ILayerViewModel> layerViewModels = new List<ILayerViewModel>();
            IPersonAssignment assignment = scheduleDay.PersonAssignment();
			if (assignment != null)
            {
	            foreach (var layer in assignment.MainActivities())
	            {
		            layerViewModels.Add(new MainShiftLayerViewModel(observer, layer, assignment, eventAggregator));
	            }

	            foreach (var layer in assignment.OvertimeActivities())
	            {
								layerViewModels.Add(new OvertimeLayerViewModel(observer, layer, assignment, eventAggregator));
	            }

	            foreach (var personalLayer in assignment.PersonalActivities())
	            {
		            layerViewModels.Add(new PersonalShiftLayerViewModel(observer, personalLayer, assignment, eventAggregator));
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
