using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection
{
	public partial class DateSelectionComposite
	{
		public DateSelectionComposite()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
		  }
		 
		public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;
	
		internal void AddSelectedDates(IList<DateOnlyPeriod> selectedDates)
		{
			periodListSelectionBox1.AddSelectedDates(selectedDates);
		}

		private void btnApplyClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;

			var handler = DateRangeChanged;
			if (handler!= null)
			{
				if (periodListSelectionBox1.SelectedDates.Count > 0)
				{
					var dateRangeChangedEventArgs = new DateRangeChangedEventArgs(periodListSelectionBox1.SelectedDates);
					handler(this, dateRangeChangedEventArgs);
				}
			}

			Cursor = Cursors.Default;
		}

		[DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
		public bool ShowDateSelectionRolling
		{
			get { return dateSelectionControl1.ShowDateSelectionRolling; }
			set { dateSelectionControl1.ShowDateSelectionRolling = value; }
		}

		[DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
		public bool ShowDateSelectionFromTo
		{
			get { return dateSelectionControl1.ShowDateSelectionRolling; }
			set { dateSelectionControl1.ShowDateSelectionRolling = value; }
		}

		[DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
		public bool ShowDateSelectionCalendar
		{
			get { return dateSelectionControl1.ShowDateSelectionRolling; }
			set { dateSelectionControl1.ShowDateSelectionRolling = value; }
		}

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

		private void dateSelectionControl1AddButtonClicked(object sender, DateRangeChangedEventArgs e)
		{
			periodListSelectionBox1.AddSelectedDates(new List<DateOnlyPeriod>(e.SelectedDates));
		}
	}
}