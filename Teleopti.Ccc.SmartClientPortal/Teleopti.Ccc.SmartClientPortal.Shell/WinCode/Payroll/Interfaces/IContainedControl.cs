using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces
{
    public interface IContainedControl
    {
        /// <summary>
        /// Gets or sets the common behaviour instance.
        /// </summary>
        /// <value>The common behaviour instance.</value>
        ICommonBehavior CommonBehaviorInstance { get; }

        /// <summary>
        /// Gets the contained control.
        /// </summary>
        /// <value>The contained control.</value>
        UserControl UserControl { get; }

    }
}
