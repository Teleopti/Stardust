using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    /// <summary>
    /// Base view interface
    /// </summary>
    public interface IBaseView
    {
        /// <summary>
        /// Gets the explorer view.
        /// </summary>
        /// <value>The explorer view.</value>
        IExplorerView ExplorerView { get; }
    }
}
