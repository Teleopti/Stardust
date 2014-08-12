using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.Win.Forecasting.Forms.WFControls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    public partial class DateSelectionComposite
    {
        public DateSelectionComposite()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
		  }
		 

        public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

        private void dateSelectionControl1_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            periodListSelectionBox1.AddSelectedDates(new List<DateOnlyPeriod>(e.SelectedDates));
        }

        
        internal void AddSelectedDates(IList<DateOnlyPeriod> selectedDates)
        {
            periodListSelectionBox1.AddSelectedDates(selectedDates);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

        	var handler = DateRangeChanged;
            if (handler!= null)
            {
                if (periodListSelectionBox1.SelectedDates.Count > 0)
                {
                    DateRangeChangedEventArgs dateRangeChangedEventArgs = new DateRangeChangedEventArgs(periodListSelectionBox1.SelectedDates);
                    handler(this, dateRangeChangedEventArgs);
                }
            }

            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show date selection rolling].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show date selection rolling]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        [DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
        public bool ShowDateSelectionRolling
        {
            get { return dateSelectionControl1.ShowDateSelectionRolling; }
            set { dateSelectionControl1.ShowDateSelectionRolling = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show date selection from to].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show date selection from to]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        [DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
        public bool ShowDateSelectionFromTo
        {
            get { return dateSelectionControl1.ShowDateSelectionRolling; }
            set { dateSelectionControl1.ShowDateSelectionRolling = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show date selection calendar].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show date selection calendar]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        [DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
        public bool ShowDateSelectionCalendar
        {
            get { return dateSelectionControl1.ShowDateSelectionRolling; }
            set { dateSelectionControl1.ShowDateSelectionRolling = value; }
        }

        /// <summary>
        /// Gets or sets the button apply text.
        /// </summary>
        /// <value>The button apply text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        [DefaultValue("xxApply"), Browsable(true), Category("Teleopti Appearance"), Localizable(true)]
        public string ButtonApplyText
        {
            get { return btnApply.Text; }
            set { btnApply.Text = value; }
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        public int SelectedDatesCount
        {
            get { return periodListSelectionBox1.SelectedDates.Count; }
        }

        public void AddControlToLastRow(Control control, int row)
		{
			tableLayoutPanel1.RowCount = row + 1;
			tableLayoutPanel1.Controls.Add(control, tableLayoutPanel1.ColumnCount - 1, row);
			//tableLayoutPanel1.RowStyles.Add(tableLayoutPanel1.RowStyles[row - 1]);
		}

        internal void SetSelectedDates(IList<DateOnlyPeriod> selectedDates)
        {
            periodListSelectionBox1.SetSelectedDates(selectedDates);
        }

    }
}