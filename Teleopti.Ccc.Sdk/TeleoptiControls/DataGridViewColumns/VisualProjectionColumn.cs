using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GridTest;

namespace TeleoptiControls.DataGridViewColumns
{
    public class VisualProjectionColumn : DataGridViewColumn
    {
        private TimePeriod _viewSpan = new TimePeriod(TimeSpan.FromHours(6), TimeSpan.FromHours(21));
        private string _dispalyDate = string.Empty;

        public VisualProjectionColumn() :base()
        {
            base.DefaultHeaderCellType = typeof(VisualProjectionHeaderCell);
            VisualProjectionCell myTemplate = new VisualProjectionCell();
            base.CellTemplate = myTemplate;

            MinimumWidth = 2;
            Width = 100;
        }

        public TimePeriod ViewSpan
        {
            get { return _viewSpan; }
            set { _viewSpan = value; }
        }

        public string DispalyDate
        {
            get { return _dispalyDate; }
            set { _dispalyDate = value; }
        }
    }
}
