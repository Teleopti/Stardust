namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    /// <summary>
    /// Target for ShiftEditorSettings
    /// </summary>
    public interface IShiftEditorSettingsTarget
    {
        /// <summary>
        /// Settingses the altered.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <remarks>
        /// Called when the settings are changed
        /// </remarks>
        void SettingsAltered(ShiftEditorSettings settings);

        ShiftEditorSettings Settings { get; }
    }
}