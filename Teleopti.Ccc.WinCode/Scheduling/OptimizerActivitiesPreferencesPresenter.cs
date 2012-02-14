using Teleopti.Interfaces.Domain;
using System.Collections.Generic;
using System;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class OptimizerActivitiesPreferencesPresenter
    {
        IOptimizerActivitiesPreferences _model;
        IOptimizerActivitiesPreferencesView _view;
        private bool _isCanceled = true;
        private int _resolution;

        public OptimizerActivitiesPreferencesPresenter(IOptimizerActivitiesPreferences model, IOptimizerActivitiesPreferencesView view, int resolution)
        {
            _model = model;
            _view = view;
            _resolution = resolution;
        }

        public void Initialize()
        {
            _view.Initialize(_resolution, _model.Activities, _model.DoNotMoveActivities);
            _view.KeepShiftCategory(_model.KeepShiftCategory);
            _view.KeepStartTime(_model.KeepStartTime);
            _view.KeepEndTime(_model.KeepEndTime);
            _view.KeepBetween(_model.AllowAlterBetween);
            
        }

        public bool IsCanceled
        {
            get { return _isCanceled; }
        }

        public void OnButtonCancelClick()
        {
            _view.HideForm();
        }

        public void OnButtonOkClick(bool alterBetween, TimeSpan startTime, TimeSpan endTime)
        {
            if (alterBetween)
            {
                TimePeriod timePeriod;

                if (startTime >= endTime)
                {
                    timePeriod = new TimePeriod(startTime, endTime.Add(TimeSpan.FromDays(1)));
                }
                else
                {
                    timePeriod = new TimePeriod(startTime, endTime);
                }

                OnKeepBetweenChanged(timePeriod);
            }
            else
            {
                OnKeepBetweenChanged(null);
            }

            OnDoNotMoveActivitiesChanged(_view.DoNotMoveActivities());

            _isCanceled = false;
            _view.HideForm();
        }

        public void OnKeepShiftCategoryCheckedChanged(bool check)
        {
            _model.KeepShiftCategory = check;   
        }

        public void OnKeepStartTimeCheckedChanged(bool check)
        {
            _model.KeepStartTime = check;
        }

        public void OnKeepEndTimeCheckedChanged(bool check)
        {
            _model.KeepEndTime = check;
        }

        public void OnKeepBetweenChanged(TimePeriod? period)
        {
            _model.AllowAlterBetween = period;
        }

        public void OnDoNotMoveActivitiesChanged(IList<IActivity> activities)
        {
            _model.SetDoNotMoveActivities(activities);
        }
    }
}
