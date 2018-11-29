using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public interface IAddLayerViewModel<T>:IDialogResult
    {
        /// <summary>
        /// Gets or  the selected period.
        /// </summary>
        /// <value>The selected period.</value>
        DateTimePeriod SelectedPeriod
        { get;

        }

        /// <summary>
        /// Gets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        T SelectedItem
        { get;
        }

    }
}