using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard
{
    public class ClipboardSpecialPresenter
    {
        private IClipboardSpecialView _view;
        private PasteOptions _model;
        private bool _deleteMode;
        private bool _showRestrictions;

        public ClipboardSpecialPresenter(IClipboardSpecialView view, PasteOptions model, bool deleteMode, bool showRestrictions)
        {
            _view = view;
            _model = model;
            _deleteMode = deleteMode;
            _showRestrictions = showRestrictions;
        }

        public void Initialize()
        {
            _view.SetTexts();
            _view.SetColor();
            _view.SetPermissionOnAbsences(PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence));
			_view.SetPermissionOnDayOffs(PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
            _view.SetPermissionOnPersonalAssignments(PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
            _view.SetPermissionOnAssignments(PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
            _view.SetPermissionOnOvertime(PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
            _view.SetPermissionsOnRestrictions(PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction));
			_view.SetPermissionsOnShiftAsOvertime(PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
            _view.ShowRestrictions(_showRestrictions);
        }

        public bool IsCanceled()
        {
            return _model != null ? false : true;
        }

        public void OnButtonOkClick()
        {
            _view.HideForm();
        }

        public void OnButtonCancelClick()
        {
            _model = null;
            _view.HideForm();
        }

        public void OnCheckBoxAssignmentsCheckedChanged(bool check)
        {
	        if (!_deleteMode)
	        {
		        _model.MainShift = check;
	        }
	        else
	        {
		        _model.MainShiftSpecial = check;
	        }
        }

        public void OnCheckBoxAbsencesCheckedChanged(bool check)
        {
            if (check)
            {
                if (_deleteMode)
                    _model.Absences = PasteAction.Replace;
                else
                    _model.Absences = PasteAction.Add;
            }
            else
                _model.Absences = PasteAction.Ignore;
        }

        public void OnCheckBoxDayOffsCheckedChanged(bool check)
        {
            _model.DayOff = check;
        }

        public void OnCheckBoxPersonalAssignmentsCheckedChanged(bool check)
        {
            _model.PersonalShifts = check;
        }

        public void OnCheckBoxOvertimeCheckedChanged(bool check)
        {
            _model.Overtime = check;
        }

        public void OnCheckBoxPreferencesCheckedChanged(bool check)
        {
            _model.Preference = check;
        }

        public void OnCheckBoxStudentAvailabilityCheckedChange(bool check)
        {
            _model.StudentAvailability = check;
        }

        public void OnCheckBoxOvertimeAvailabilityCheckedChanged(bool check)
        {
            _model.OvertimeAvailability = check;
        }

		public void OnCheckBoxShiftAsOvertimeCheckedChanged(bool check)
		{
			_model.ShiftAsOvertime = check;
		}

		public void OnComboBoxAdvOvertimeSelectedIndexChanged(IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
		{
			_model.MulitiplicatorDefinitionSet = multiplicatorDefinitionSet;
		}
    }
}
