using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
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

        public IAddLayerViewModel<IAbsence> CreateAddAbsenceViewModel(IEnumerable<IAbsence> bindingList, ISetupDateTimePeriod period, TimeZoneInfo timeZoneInfo)
        {
            var model = new AddAbsenceViewModel(bindingList, period, WorkingInterval);
            model.PeriodViewModel.Min = Presenter.SelectedPeriod.Period().StartDateTime;
            model.PeriodViewModel.Max = Presenter.SelectedPeriod.Period().EndDateTime.AddDays(2);
            AddDialogComposer<AddAbsenceViewModel> composer = new AddDialogComposer<AddAbsenceViewModel>(model, timeZoneInfo);
            return composer.Result();
        }

        public IAddLayerViewModel<IActivity> CreateAddPersonalActivityViewModel(IEnumerable<IActivity> activities, DateTimePeriod period, TimeZoneInfo timeZoneInfo)
        {
            var model = new AddPersonalActivityViewModel(activities, period, WorkingInterval);
            SetBoundary(model);
            AddDialogComposer<IAddLayerViewModel<IActivity>> composer = new AddDialogComposer<IAddLayerViewModel<IActivity>>(model, timeZoneInfo);
            return composer.Result();
        }

        public IAddOvertimeViewModel CreateAddOvertimeViewModel(IEnumerable<IActivity> activities, IList<IMultiplicatorDefinitionSet> definitionSets, IActivity defaultActivity, DateTimePeriod period, TimeZoneInfo timeZoneInfo)
        {
            var fallbackDefaultHours = new SetupDateTimePeriodToDefaultPeriod(period);
            var model = new AddOvertimeViewModel(activities, definitionSets, defaultActivity, fallbackDefaultHours, WorkingInterval);
            SetBoundary(model);
            return new AddDialogComposer<AddOvertimeViewModel>(model, timeZoneInfo).Result();
        }

        public IAddLayerViewModel<IDayOffTemplate> CreateAddDayOffViewModel(IEnumerable<IDayOffTemplate> dayOffTemplates, TimeZoneInfo timeZoneInfo, DateTimePeriod period)
        {
            var model = new AddDayOffViewModel(dayOffTemplates, period);
            SetBoundary(model);
            return new AddDialogComposer<AddDayOffViewModel>(model, timeZoneInfo).Result();
        }

        public IAddActivityViewModel CreateAddActivityViewModel(IEnumerable<IActivity> activities, IList<IShiftCategory> shiftCategories, DateTimePeriod period, TimeZoneInfo timeZoneInfo, IActivity defaultActivity)
        {
            var model = new AddActivityViewModel(activities, shiftCategories, period, WorkingInterval, defaultActivity);
            SetBoundary(model);
            return new AddDialogComposer<AddActivityViewModel>(model, timeZoneInfo).Result();
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