using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class RequestAllowanceGridControl : GridControl
    {
        public const int NavigationButtonRowIndex = 0;
        public const int PrevButtonColIndex = 1;
        public const int NextButtonColIndex = 7;

        public RequestAllowanceGridControl()
        {
            InitializeComponent();
            CellButtonClicked += gridControlCellButtonClicked;
        }

        private void gridControlCellButtonClicked(object sender, GridCellButtonClickedEventArgs e)
        {
            if (e.RowIndex != NavigationButtonRowIndex) return;
            if (e.ColIndex == PrevButtonColIndex)
                OnPreviousButtonClicked(null);
            else if (e.ColIndex == NextButtonColIndex)
                OnNextButtonClicked(null);
        }

        public event EventHandler<EventArgs> PreviousButtonClicked;
        public event EventHandler<EventArgs> NextButtonClicked;

        public void OnNextButtonClicked(EventArgs e)
        {
            var handler = NextButtonClicked;
            if (handler != null) handler(this, e);
        }

        public void OnPreviousButtonClicked(EventArgs e)
        {
            var handler = PreviousButtonClicked;
            if (handler != null) handler(this, e);
        }
    }
}
