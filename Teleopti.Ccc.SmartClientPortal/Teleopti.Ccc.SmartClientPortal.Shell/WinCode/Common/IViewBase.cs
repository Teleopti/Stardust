using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    /// <summary>
    /// Base interface for all views
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-11-05
    /// </remarks>
    public interface IViewBase
    {
        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-05
        /// </remarks>
        void ShowErrorMessage(string text,string caption);

        /// <summary>
        /// Shows the confirmation message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-19
        /// </remarks>
        DialogResult ShowConfirmationMessage(string text, string caption);

        DialogResult ShowYesNoMessage(string text, string caption);

        /// <summary>
        /// Shows the information message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-19
        /// </remarks>
        void ShowInformationMessage(string text, string caption);


        DialogResult ShowWarningMessage(string text, string caption);
    }
}