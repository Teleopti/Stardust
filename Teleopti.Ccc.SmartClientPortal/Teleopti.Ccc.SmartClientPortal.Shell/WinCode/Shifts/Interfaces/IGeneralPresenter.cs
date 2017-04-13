namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IGeneralPresenter : IValidate
    {
        IGeneralTemplatePresenter GeneralTemplatePresenter { get; }

        IActivityPresenter ActivityPresenter { get; }

        IAccessibilityDatePresenter AccessibilityDatePresenter { get; }

        IActivityTimeLimiterPresenter ActivityTimeLimiterPresenter { get; }

        IDaysOfWeekPresenter DaysOfWeekPresenter { get; }

        void LoadModelCollection();

    }
}
