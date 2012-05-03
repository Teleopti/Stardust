using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WpfControls.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-31
    /// </remarks>
    public abstract class AddScheduleLayers : ViewBase<SchedulePresenterBase>, IAddScheduleLayers
    {
        protected AddScheduleLayers(SchedulePresenterBase presenter)
            : base(presenter)
        { }

        public IAddLayerViewModel<IAbsence> CreateAddAbsenceViewModel(IList<IAbsence> bindingList, ISetupDateTimePeriod period)
        {
            var model = new AddAbsenceViewModel(bindingList, period, WorkingInterval);
            model.PeriodViewModel.Min = Presenter.SelectedPeriod.Period().StartDateTime;
            model.PeriodViewModel.Max = Presenter.SelectedPeriod.Period().EndDateTime.AddDays(2);
            AddDialogComposer<AddAbsenceViewModel> composer = new AddDialogComposer<AddAbsenceViewModel>(model);
            return composer.Result();
        }

        public IAddLayerViewModel<IActivity> CreateAddPersonalActivityViewModel(IList<IActivity> activities, DateTimePeriod period, ICccTimeZoneInfo timeZoneInfo)
        {
            var model = new AddPersonalActivityViewModel(activities, period, WorkingInterval);
            SetBoundary(model);
            AddDialogComposer<IAddLayerViewModel<IActivity>> composer = new AddDialogComposer<IAddLayerViewModel<IActivity>>(model);
            return composer.Result();
        }

        public IAddOvertimeViewModel CreateAddOvertimeViewModel(IScheduleDay selectedSchedule, IList<IActivity> activities, IList<IMultiplicatorDefinitionSet> definitionSets, IActivity defaultActivity, DateTimePeriod period)
        {
            var dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(period.LocalStartDateTime, period.LocalEndDateTime);
            var fallbackDefaultHours = new SetupDateTimePeriodDefaultLocalHoursForActivities(selectedSchedule, dateTimePeriod);
            var model = new AddOvertimeViewModel(activities, definitionSets, defaultActivity, fallbackDefaultHours, WorkingInterval);
            SetBoundary(model);
            return new AddDialogComposer<AddOvertimeViewModel>(model).Result();
        }

        public IAddLayerViewModel<IDayOffTemplate> CreateAddDayOffViewModel(IList<IDayOffTemplate> dayOffTemplates, ICccTimeZoneInfo timeZoneInfo, DateTimePeriod period)
        {
            var model = new AddDayOffViewModel(dayOffTemplates, period);
            SetBoundary(model);
            return new AddDialogComposer<AddDayOffViewModel>(model).Result();
        }

        public IAddActivityViewModel CreateAddActivityViewModel(IList<IActivity> activities, IList<IShiftCategory> shiftCategories, DateTimePeriod period, ICccTimeZoneInfo timeZoneInfo)
        {
            var model = new AddActivityViewModel(activities, shiftCategories, period, WorkingInterval);
            SetBoundary(model);
            return new AddDialogComposer<AddActivityViewModel>(model).Result();
        }

        private static TimeSpan WorkingInterval
        {
            get { return TimeSpan.FromMinutes(15); }
        }

        private void SetBoundary<T>(AddLayerViewModel<T> model) where T : class
        {
            model.PeriodViewModel.Min = Presenter.SelectedPeriod.Period().StartDateTime;
            model.PeriodViewModel.Max = Presenter.SelectedPeriod.Period().EndDateTime;
        }
    }
}