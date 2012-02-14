using System;
using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public class EditStudentAvailabilityPresenter
    {
        private readonly IEditStudentAvailabilityView _view;
        private readonly IEditStudentAvailabilityModel _model;
        private IStudentAvailabilityCellData _cellData;
        private bool _isInTrigger;

        public EditStudentAvailabilityPresenter(IEditStudentAvailabilityView view, IEditStudentAvailabilityModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            ClearValidationErrors();
            _view.SaveButtonEnabled = false;
            if (!_model.CreateStudentAvailabilityIsPermitted)
            {
                _view.HideView();
            }
        }

        private void ClearValidationErrors()
        {
            _view.StartTimeLimitationErrorMessage = null;
            _view.EndTimeLimitationErrorMessage = null;
            _view.SecondStartTimeLimitationErrorMessage = null;
            _view.SecondEndTimeLimitationErrorMessage = null;
        }

        public event EventHandler<SaveStudentAvailabilityCellDataEventArgs> SaveStudentAvailabilityCellData;
        
        public void SetStudentAvailabilityRestrictions(IStudentAvailabilityCellData cellData)
        {
            AvoidRecursiveTriggering(() =>
                                         {
                                             ClearValidationErrors();

                                             IList<StudentAvailabilityRestriction> studentAvailabilityResctrictions = null;
                                             _cellData = cellData;
                                             if (_cellData != null)
                                             {
                                                 studentAvailabilityResctrictions = cellData.StudentAvailabilityRestrictions;
                                             }

                                             _model.SetStudentAvailabilityRestrictions(studentAvailabilityResctrictions);
                                             SetModelValuesToView();

                                             _view.SaveButtonEnabled = false;
                                         });
        }

        public void SetModelValuesToView()
        {
            _view.StartTimeLimitation = _model.StartTimeLimitation;
            _view.EndTimeLimitation = _model.EndTimeLimitation;
            _view.EndTimeLimitationNextDay = _model.EndTimeLimitationNextDay;
            _view.SecondStartTimeLimitation = _model.SecondStartTimeLimitation;
            _view.SecondEndTimeLimitation = _model.SecondEndTimeLimitation;
            _view.SecondEndTimeLimitationNextDay = _model.SecondEndTimeLimitationNextDay;
        }

        public void SetViewValuesToModel()
        {
            _model.StartTimeLimitation = _view.StartTimeLimitation;
            _model.EndTimeLimitation = _view.EndTimeLimitation;
            _model.EndTimeLimitationNextDay = _view.EndTimeLimitationNextDay;
            _model.SecondStartTimeLimitation = _view.SecondStartTimeLimitation;
            _model.SecondEndTimeLimitation = _view.SecondEndTimeLimitation;
            _model.SecondEndTimeLimitationNextDay = _view.SecondEndTimeLimitationNextDay;
        }

        public void Save()
        {
            if (Validate())
            {
                SetViewValuesToModel();
                _model.SetValuesToStudentAvailabilityRestrictions();
                _cellData.StudentAvailabilityRestrictions = _model.StudentAvailabilityRestrictions;
                OnSaveStudentAvailabilityCellData();
                _view.SaveButtonEnabled = false;
            }
        }

        private void OnSaveStudentAvailabilityCellData()
        {
            if (SaveStudentAvailabilityCellData != null)
            {
                SaveStudentAvailabilityCellData.Invoke(this, new SaveStudentAvailabilityCellDataEventArgs(_cellData));
            }
        }

        private bool Validate()
        {
            return CheckInputIsNoneOrAll() && CheckEndTimeLargerThanStartTime();
        }

        private bool CheckEndTimeLargerThanStartTime()
        {
            ClearValidationErrors();
            var valid = true;
            if ((_view.StartTimeLimitation > _view.EndTimeLimitation) && !_view.EndTimeLimitationNextDay)
            {
                var errorMessage = Resources.EndTimeMustBeGreaterOrEqualToStartTime;
                _view.EndTimeLimitationErrorMessage = errorMessage;
                valid = false;
            }
            if ((_view.SecondStartTimeLimitation > _view.SecondEndTimeLimitation) && !_view.SecondEndTimeLimitationNextDay)
            {
                var errorMessage = Resources.EndTimeMustBeGreaterOrEqualToStartTime;
                _view.SecondEndTimeLimitationErrorMessage = errorMessage;
                valid = false;
            }
            return valid;
        }

        private bool CheckInputIsNoneOrAll()
        {
            ClearValidationErrors();
            var valid = true;
            if (!_view.StartTimeLimitation.HasValue && _view.EndTimeLimitation.HasValue)
            {
                var errorMessage = Resources.MustSpecifyValidTime;
                _view.StartTimeLimitationErrorMessage = errorMessage;
                valid = false;
            }
            else if (_view.StartTimeLimitation.HasValue && !_view.EndTimeLimitation.HasValue)
            {
                var errorMessage = Resources.MustSpecifyValidTime;
                _view.EndTimeLimitationErrorMessage = errorMessage;
                valid = false;
            }

            if (!_view.SecondStartTimeLimitation.HasValue && _view.SecondEndTimeLimitation.HasValue)
            {
                var errorMessage = Resources.MustSpecifyValidTime;
                _view.SecondStartTimeLimitationErrorMessage = errorMessage;
                valid = false;
            }
            else if (_view.SecondStartTimeLimitation.HasValue && !_view.SecondEndTimeLimitation.HasValue)
            {
                var errorMessage = Resources.MustSpecifyValidTime;
                _view.SecondEndTimeLimitationErrorMessage = errorMessage;
                valid = false;
            }
            return valid;
        }

        private bool HasCellData()
        {
            return _cellData != null;
        }

        public void AllowInput(bool enable)
        {
            _view.AllowInput(enable);
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
}