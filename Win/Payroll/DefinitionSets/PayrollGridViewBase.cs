using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    public class PayrollGridViewBase : PayrollBaseUserControl
    {

        /// <summary>
        /// Gets the person's selected UI culture.
        /// </summary>
        public static CultureInfo CurrentUICulture
        {
            get
            {
                return TeleoptiPrincipal.Current.Regional.UICulture;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollGridViewBase"/> class.
        /// </summary>
        public PayrollGridViewBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollGridViewBase"/> class.
        /// </summary>
        /// <param name="explorerView">The explorer view.</param>
        public PayrollGridViewBase(IExplorerView explorerView): base(explorerView)
        {

        }

        /// <summary>
        /// Determines whether [is ready to delete] [the specified grid helper].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gridHelper">The grid helper.</param>
        /// <returns>
        /// 	<c>true</c> if [is ready to delete] [the specified grid helper]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReadyToDelete<T>(SFGridColumnGridHelper<T> gridHelper)
        {
            bool isReady = false;

            if (IsDataAvailable(gridHelper))
            {
                if (ShowMyErrorMessage(UserTexts.Resources.MultiplicatorDefinitionDeleteConfirmation,
                    UserTexts.Resources.Message) == DialogResult.Yes)
                {
                    isReady = true;
                }
            }

            return isReady;
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        public void ShowErrorMessage(string message, string caption)
        {

            Syncfusion.Windows.Forms.MessageBoxAdv.Show(
                 string.Concat(message, "  "),
                 caption,
                 MessageBoxButtons.OK,
                 MessageBoxIcon.Warning,
                 MessageBoxDefaultButton.Button1,
                 (RightToLeft == RightToLeft.Yes)
                                  ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                  : 0);
        }

        /// <summary>
        /// Determines whether [is data available].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is data available]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsDataAvailable<T>(SFGridColumnGridHelper<T> _gridHelper)
        {
            return (_gridHelper.SourceList != null) && (_gridHelper.SourceList.Count > 0);
        }

        /// <summary>
        /// Shows my error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        /// <returns></returns>
        private DialogResult ShowMyErrorMessage(string message, string caption)
        {
			// no bloody sunkfusion
            return MessageBox.Show(
                   string.Concat(message, "  "),
                   caption,
                   MessageBoxButtons.YesNo,
                   MessageBoxIcon.Warning,
                   MessageBoxDefaultButton.Button1,
                   (RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
        }

        public override string ToolTipDelete
        {
            get { return Resources.DeleteMultiplicator; }
        }

        public override string ToolTipAddNew
        {
            get { return Resources.AddNewMultiplicator; }
        }
    }
}
