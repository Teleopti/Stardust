using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    public class PeriodListSelectionBox : BaseUserControl
    {
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemove;
        private Syncfusion.Windows.Forms.Grid.GridListControl gridListControlPeriods;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvClear;
        private TableLayoutPanel tableLayoutPanel1;
		private System.ComponentModel.IContainer components;

        private readonly IList<DateOnlyPeriod> _dateTimePairs = new List<DateOnlyPeriod>();

        public PeriodListSelectionBox()
        {
            InitializeComponent();
            GridStyleInfoExtensions.ResetDefault();
            if (!DesignMode) SetTexts();

            gridListControlPeriods.DataSource = _dateTimePairs;
            gridListControlPeriods.DisplayMember = "DateString";
            SetButtonEnableStatus();
        }

        /// <summary>
        /// Gets the selected dates.
        /// </summary>
        /// <value>The selected dates.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        internal ReadOnlyCollection<DateOnlyPeriod> SelectedDates
        {
            get
            {
                return new ReadOnlyCollection<DateOnlyPeriod>(_dateTimePairs);
            }
        }

        /// <summary>
        /// Adds the selected dates.
        /// </summary>
        /// <param name="dateTimePairs">The date time pairs.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-15
        /// </remarks>
        internal void AddSelectedDates(IList<DateOnlyPeriod> dateTimePairs)
        {
            foreach (DateOnlyPeriod dtp in dateTimePairs)
                _dateTimePairs.Add(dtp);

            rebindGrid();
            SetButtonEnableStatus();
        }

        #region InitializeComponent

        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.buttonAdvRemove = new Syncfusion.Windows.Forms.ButtonAdv();
			this.gridListControlPeriods = new Syncfusion.Windows.Forms.Grid.GridListControl();
			this.buttonAdvClear = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.gridListControlPeriods)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonAdvRemove
			// 
			this.buttonAdvRemove.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonAdvRemove.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(155)))), ((int)(((byte)(255)))));
			this.buttonAdvRemove.BeforeTouchSize = new System.Drawing.Size(76, 23);
			this.buttonAdvRemove.Enabled = false;
			this.buttonAdvRemove.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRemove.IsBackStageButton = false;
			this.buttonAdvRemove.Location = new System.Drawing.Point(4, 2);
			this.buttonAdvRemove.Margin = new System.Windows.Forms.Padding(2);
			this.buttonAdvRemove.Name = "buttonAdvRemove";
			this.buttonAdvRemove.Size = new System.Drawing.Size(76, 23);
			this.buttonAdvRemove.TabIndex = 4;
			this.buttonAdvRemove.Text = "xxDelete";
			this.buttonAdvRemove.UseVisualStyle = true;
			this.buttonAdvRemove.Click += new System.EventHandler(this.buttonAdvRemove_Click);
			// 
			// gridListControlPeriods
			// 
			this.gridListControlPeriods.AllowResizeColumns = false;
			this.gridListControlPeriods.BackColor = System.Drawing.SystemColors.Control;
			this.gridListControlPeriods.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tableLayoutPanel1.SetColumnSpan(this.gridListControlPeriods, 2);
			this.gridListControlPeriods.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridListControlPeriods.FillLastColumn = true;
			this.gridListControlPeriods.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridListControlPeriods.ItemHeight = 14;
			this.gridListControlPeriods.Location = new System.Drawing.Point(3, 31);
			this.gridListControlPeriods.MultiColumn = false;
			this.gridListControlPeriods.Name = "gridListControlPeriods";
			this.gridListControlPeriods.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridListControlPeriods.Properties.ForceImmediateRepaint = false;
			this.gridListControlPeriods.Properties.MarkColHeader = false;
			this.gridListControlPeriods.Properties.MarkRowHeader = false;
			this.gridListControlPeriods.SelectedIndex = -1;
			this.gridListControlPeriods.ShowColumnHeader = false;
			this.gridListControlPeriods.Size = new System.Drawing.Size(165, 113);
			this.gridListControlPeriods.TabIndex = 6;
			this.gridListControlPeriods.ThemesEnabled = true;
			this.gridListControlPeriods.TopIndex = 0;
			this.gridListControlPeriods.SelectedValueChanged += new System.EventHandler(this.gridListControlPeriods_SelectedValueChanged);
			// 
			// buttonAdvClear
			// 
			this.buttonAdvClear.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonAdvClear.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(155)))), ((int)(((byte)(255)))));
			this.buttonAdvClear.BeforeTouchSize = new System.Drawing.Size(76, 23);
			this.buttonAdvClear.ForeColor = System.Drawing.Color.White;
			this.buttonAdvClear.IsBackStageButton = false;
			this.buttonAdvClear.Location = new System.Drawing.Point(90, 2);
			this.buttonAdvClear.Margin = new System.Windows.Forms.Padding(2);
			this.buttonAdvClear.Name = "buttonAdvClear";
			this.buttonAdvClear.Size = new System.Drawing.Size(76, 23);
			this.buttonAdvClear.TabIndex = 5;
			this.buttonAdvClear.Text = "xxClear";
			this.buttonAdvClear.UseVisualStyle = true;
			this.buttonAdvClear.Click += new System.EventHandler(this.buttonAdvClear_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.gridListControlPeriods, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvRemove, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvClear, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(171, 147);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// PeriodListSelectionBox
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "PeriodListSelectionBox";
			this.Padding = new System.Windows.Forms.Padding(2);
			this.Size = new System.Drawing.Size(175, 151);
			((System.ComponentModel.ISupportInitialize)(this.gridListControlPeriods)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private void SetButtonEnableStatus()
        {
            if (gridListControlPeriods.Items.Count == 0)
            {
                buttonAdvRemove.Enabled = false;
                buttonAdvClear.Enabled = false;
            }
            else
            {
               // buttonAdvRemove.Enabled = true;
                buttonAdvClear.Enabled = true;
            }
        }

        private void rebindGrid()
        {
            gridListControlPeriods.DataSource = null;
            gridListControlPeriods.DataSource = _dateTimePairs;
            gridListControlPeriods.DisplayMember = "DateString";
        }

        private void buttonAdvRemove_Click(object sender, EventArgs e)
        {

            int selectedIndex = gridListControlPeriods.SelectedIndex;
            if (selectedIndex >= 0)
            {
                gridListControlPeriods.Items.RemoveAt(selectedIndex);
            }

            if (selectedIndex > (gridListControlPeriods.Items.Count - 1))
            {
                selectedIndex = gridListControlPeriods.Items.Count - 1;
            }
              gridListControlPeriods.SelectedIndex = selectedIndex;
            SetButtonEnableStatus();
            gridListControlPeriods.Refresh();
        }

        private void gridListControlPeriods_SelectedValueChanged(object sender, EventArgs e)
        {
            if (gridListControlPeriods.SelectedItem != null)
            {
                SetButtonEnableStatus();
                buttonAdvRemove.Enabled = true;
            }
        }

        private void buttonAdvClear_Click(object sender, EventArgs e)
        {
            gridListControlPeriods.Items.Clear();
            gridListControlPeriods.SelectedIndex = -1;
            SetButtonEnableStatus();
            gridListControlPeriods.Refresh();
        }

        /// <summary>
        /// Sets the selected dates.
        /// </summary>
        /// <param name="dateTimePairs">The date time pairs.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        internal void SetSelectedDates(IList<DateOnlyPeriod> dateTimePairs)
        {
            gridListControlPeriods.Items.Clear();
            _dateTimePairs.Clear();
            AddSelectedDates(dateTimePairs);

        }
    }
}
