using System;

namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    public interface IMultiplicatorDefinitionView
    {

        /// <summary>
        /// Configures the grid.
        /// </summary>
        void ConfigureGrid();

        /// <summary>
        /// Occurs when [grid data changed].
        /// </summary>
        event EventHandler GridDataChanged;
    }
}
