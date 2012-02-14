namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public interface IToggleButtonState
    {
        void ToggleButtonEnabled(string controlName, bool enabled);
        void ToggleButtonChecked(string controlName, bool? isChecked);
        //ToDo: remove to somewhere
        void SetMustHaveText(string mustHaveInfo);
        void ShowRightPanel(bool show);
    }
}
