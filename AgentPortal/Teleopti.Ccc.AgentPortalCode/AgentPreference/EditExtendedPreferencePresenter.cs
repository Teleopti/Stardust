using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public class EditExtendedPreferencePresenter
    {
        private readonly IEditExtendedPreferenceView _view;
        private readonly IExtendedPreferenceModel _model;
        private IPreferenceCellData _cellData;
        private bool _isInTrigger;

        public EditExtendedPreferencePresenter(IEditExtendedPreferenceView view, IExtendedPreferenceModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            var allowdPreferenceActivities = _model.AllowedPreferenceActivities;
            var displayActivities = allowdPreferenceActivities != null;

            ClearValidationErrors();
            _view.DayOffEnabled = true;
            _view.ShiftCategoryEnabled = true;
            _view.AbsenceEnabled = true;
            _view.ShiftTimeControlsEnabled = false;
            _view.ActivityEnabled = true;
            _view.ActivityTimeControlsEnabled = false;
            _view.SaveButtonEnabled = false;
            _view.PopulateActivities(allowdPreferenceActivities);

            if (!_model.ModifyExtendedPreferencesIsPermitted)
            {
                _view.HideView();
                OnVisibleChanged(new EditExtendedPreferencesVisibleChangedEventArgs
                                     {Visible = false, ActivityVisible = false});
            }
            else if (!displayActivities)
            {
                _view.ActivityViewVisible = false;
                OnVisibleChanged(new EditExtendedPreferencesVisibleChangedEventArgs
                                     {Visible = true, ActivityVisible = false});
            }
        }

        private void ClearValidationErrors()
        {
            _view.StartTimeLimitationErrorMessage = null;
            _view.EndTimeLimitationErrorMessage = null;
            _view.WorkTimeLimitationErrorMessage = null;
            _view.ActivityStartTimeLimitationErrorMessage = null;
            _view.ActivityEndTimeLimitationErrorMessage = null;
            _view.ActivityTimeLimitationErrorMessage = null;
        }

        private void OnVisibleChanged(EditExtendedPreferencesVisibleChangedEventArgs eventArgs)
        {
            if (VisibleChanged != null)
            {
                VisibleChanged.Invoke(this, eventArgs);
            }
        }

        public event EventHandler<SavePreferenceCellDataEventArgs> SavePreferenceCellData;
        public event EventHandler<EditExtendedPreferencesVisibleChangedEventArgs> VisibleChanged;

        public void SetShiftCategories(IEnumerable<ShiftCategory> shiftCategories)
        {
            var list = new List<ShiftCategory>(shiftCategories);
            list.Insert(0, new ShiftCategory(Resources.None, string.Empty, null, Color.Empty));
            _view.PopulateShiftCategories(list);
        }

        public void SetDaysOff(IEnumerable<DayOff> daysOff)
        {
            var list = new List<DayOff>(daysOff);
            list.Insert(0, new DayOff(Resources.None, string.Empty, null, Color.Empty));
            _view.PopulateDaysOff(list);
        }

        public void SetAbsences(IEnumerable<Absence> absences)
        {
            var list = new List<Absence>(absences);
            list.Insert(0, new Absence(Resources.None, string.Empty, null, Color.Empty));
            _view.PopulateAbsences(list);
        }

        public void SetPreference(IPreferenceCellData cellData)
        {
            if (!_model.ModifyExtendedPreferencesIsPermitted) return;

            AvoidRecursiveTriggering(() =>
                                         {
                                             ClearValidationErrors();

                                             Preference preference = null;
                                             _cellData = cellData;
                                             if (_cellData != null)
                                             {
                                                 preference = cellData.Preference;
                                             }

                                             _model.SetPreference(preference);
                                             SetModelValuesToView();

                                             EnableDisableViewControls();
                                             _view.SaveButtonEnabled = false;
                                         });
        }

        private void EnableDisableViewControls()
        {
            if (_cellData != null && _cellData.Enabled)
            {
                _view.DayOffEnabled = true;
                if (_model.DayOff != null)
                {
                    _view.ShiftCategoryEnabled = true;
                    _view.AbsenceEnabled = true;
                    _view.ShiftTimeControlsEnabled = false;
                    _view.ActivityEnabled = true;
                    _view.ActivityTimeControlsEnabled = false;
                }
                else
                {
                    _view.ShiftCategoryEnabled = true;
                    _view.ShiftTimeControlsEnabled = true;
                    _view.AbsenceEnabled = true;
                    if (_model.AllowedPreferenceActivities != null)
                    {
                        _view.ActivityEnabled = true;
                        _view.ActivityTimeControlsEnabled = _model.Activity != null;
                    }
                    else
                    {
                        _view.ActivityEnabled = false;
                        _view.ActivityTimeControlsEnabled = false;
                    }
                }
            }
            else
            {
                _view.DayOffEnabled = false;
                _view.ShiftCategoryEnabled = false;
                _view.AbsenceEnabled = false;
                _view.ShiftTimeControlsEnabled = false;
                _view.ActivityEnabled = false;
                _view.ActivityTimeControlsEnabled = false;
            }
        }

        public void SetModelValuesToView()
        {
            _view.DayOff = _model.DayOff;
            _view.ShiftCategory = _model.ShiftCategory;
            _view.Absence = _model.Absence;
            _view.StartTimeLimitationMin = _model.StartTimeLimitationMin;
            _view.StartTimeLimitationMax = _model.StartTimeLimitationMax;
            _view.EndTimeLimitationMin = _model.EndTimeLimitationMin;
            _view.EndTimeLimitationMax = _model.EndTimeLimitationMax;
            _view.WorkTimeLimitationMin = _model.WorkTimeLimitationMin;
            _view.WorkTimeLimitationMax = _model.WorkTimeLimitationMax;
            _view.EndTimeLimitationMaxNextDay = _model.EndTimeLimitationMaxNextDay;
            _view.EndTimeLimitationMinNextDay = _model.EndTimeLimitationMinNextDay;
            _view.EndTimeLimitationMinNextDayEnabled = _model.EndTimeLimitationMin.HasValue;
            _view.EndTimeLimitationMaxNextDayEnabled = _model.EndTimeLimitationMax.HasValue;
            _view.Activity = _model.Activity;
            _view.ActivityStartTimeLimitationMin = _model.ActivityStartTimeLimitationMin;
            _view.ActivityStartTimeLimitationMax = _model.ActivityStartTimeLimitationMax;
            _view.ActivityEndTimeLimitationMin = _model.ActivityEndTimeLimitationMin;
            _view.ActivityEndTimeLimitationMax = _model.ActivityEndTimeLimitationMax;
            _view.ActivityTimeLimitationMin = _model.ActivityTimeLimitationMin;
            _view.ActivityTimeLimitationMax = _model.ActivityTimeLimitationMax;
        }

        public void SetViewValuesToModel()
        {
            _model.DayOff = _view.DayOff;
            _model.ShiftCategory = _view.ShiftCategory;
            _model.Absence = _view.Absence;
            _model.StartTimeLimitationMin = _view.StartTimeLimitationMin;
            _model.StartTimeLimitationMax = _view.StartTimeLimitationMax;
            _model.EndTimeLimitationMin = _view.EndTimeLimitationMin;
            _model.EndTimeLimitationMax = _view.EndTimeLimitationMax;
            _model.WorkTimeLimitationMin = _view.WorkTimeLimitationMin;
            _model.WorkTimeLimitationMax = _view.WorkTimeLimitationMax;
            _model.EndTimeLimitationMaxNextDay = _view.EndTimeLimitationMaxNextDay;
            _model.EndTimeLimitationMinNextDay = _view.EndTimeLimitationMinNextDay;
            _model.Activity = _view.Activity;
            _model.ActivityStartTimeLimitationMin = _view.ActivityStartTimeLimitationMin;
            _model.ActivityStartTimeLimitationMax = _view.ActivityStartTimeLimitationMax;
            _model.ActivityEndTimeLimitationMin = _view.ActivityEndTimeLimitationMin;
            _model.ActivityEndTimeLimitationMax = _view.ActivityEndTimeLimitationMax;
            _model.ActivityTimeLimitationMin = _view.ActivityTimeLimitationMin;
            _model.ActivityTimeLimitationMax = _view.ActivityTimeLimitationMax;
        }

        public void Save()
        {
            ClearValidationErrors();
            SetViewValuesToModel();
            _model.SetValuesToPreference();
            if (Validate())
            {
	            var previousStateMustHave = _cellData.Preference != null && _cellData.Preference.MustHave;
                _cellData.Preference = (Preference) _model.Preference.Clone();
	            _cellData.Preference.MustHave = previousStateMustHave;
				_cellData.Preference.TemplateName = null; 
                OnSavePreferenceCellData();
                _view.SaveButtonEnabled = false;
            }
        }

        private void OnSavePreferenceCellData()
        {
            if (SavePreferenceCellData != null)
            {
                SavePreferenceCellData.Invoke(this, new SavePreferenceCellDataEventArgs(_cellData));
            }
        }

        private bool Validate()
        {
            var error = false;
            if (!_model.Preference.StartTimeLimitation.IsValid())
            {
                _view.StartTimeLimitationErrorMessage = Resources.EndTimeMustBeGreaterOrEqualToStartTime;
                error = true;
            }
            if (!_model.Preference.EndTimeLimitation.IsValid())
            {
                _view.EndTimeLimitationErrorMessage = Resources.EndTimeMustBeGreaterOrEqualToStartTime;
                error = true;
            }
            if (!_model.Preference.WorkTimeLimitation.IsValid())
            {
                _view.WorkTimeLimitationErrorMessage = Resources.MaxWorkTimeMustBeGreaterOrEqualToMinWorkTime;
                error = true;
            }
            if (!_model.Preference.ActivityStartTimeLimitation.IsValid())
            {
                _view.ActivityStartTimeLimitationErrorMessage = Resources.EndTimeMustBeGreaterOrEqualToStartTime;
                error = true;
            }
            if (!_model.Preference.ActivityEndTimeLimitation.IsValid())
            {
                _view.ActivityEndTimeLimitationErrorMessage = Resources.EndTimeMustBeGreaterOrEqualToStartTime;
                error = true;
            }
            if (!_model.Preference.ActivityTimeLimitation.IsValid())
            {
                _view.ActivityTimeLimitationErrorMessage = Resources.MaxWorkTimeMustBeGreaterOrEqualToMinWorkTime;
                error = true;
            }
            return !error;
        }

        public void NotifyDayOffChanged(DayOff dayOff)
        {
            AvoidRecursiveTriggering(() =>
                                         {
                                             _model.DayOff = dayOff;

                                             SetModelValuesToView();
                                             EnableDisableViewControls();

                                             _view.SaveButtonEnabled = HasCellData();
                                         });
        }

        public void NotifyShiftCategoryChanged(ShiftCategory shiftCategory)
        {
            AvoidRecursiveTriggering(() =>
                                         {
                                             _model.ShiftCategory = shiftCategory;

                                             SetModelValuesToView();
                                             EnableDisableViewControls();

                                             _view.SaveButtonEnabled = HasCellData();
                                         });
        }

        public void NotifyAbsenceChanged(Absence absence)
        {
            AvoidRecursiveTriggering(() =>  
                                        { 
                                            _model.Absence = absence;
                                            SetModelValuesToView();
                                            EnableDisableViewControls();
                                            _view.SaveButtonEnabled = HasCellData();
                                        });
        }

        private bool HasCellData()
        {
            return _cellData != null;
        }

        public void NotifyActivityChanged(Activity activity)
        {
            AvoidRecursiveTriggering(() =>
                                         {
                                             _model.Activity = activity;

                                             SetModelValuesToView();
                                             EnableDisableViewControls();

                                             _view.SaveButtonEnabled = HasCellData();
                                         });
        }

        public void NotifyViewValueChanged()
        {
            AvoidRecursiveTriggering(() =>
                                         {
                                             SetViewValuesToModel();
                                             _view.SaveButtonEnabled = HasCellData();
                                         });
        }

        private delegate void Action();

        private void AvoidRecursiveTriggering(Action action)
        {
            if (!_isInTrigger)
            {
                _isInTrigger = true;
                action.Invoke();
                _isInTrigger = false;
            }
        }
    }

    public class EditExtendedPreferencesVisibleChangedEventArgs : EventArgs
    {
        public bool Visible { get; set; }
        public bool ActivityVisible { get; set; }
    }
}