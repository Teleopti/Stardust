using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class ModifyRowEventArgs:EventArgs
    {
        private TaskPeriodType _rowType;

        public ModifyRowEventArgs(TaskPeriodType rowType)
        {
            _rowType = rowType;
        }

        public TaskPeriodType RowType
        {
            get { return _rowType; }
        }
    }
}
