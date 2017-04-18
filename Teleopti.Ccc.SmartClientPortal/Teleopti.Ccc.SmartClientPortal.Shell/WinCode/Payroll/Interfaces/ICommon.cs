using System.Collections.Generic;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces
{
    public interface ICommon<T>
    {
        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <value>The view.</value>
        List<T> ModelCollection { get; set; }
    }
}
