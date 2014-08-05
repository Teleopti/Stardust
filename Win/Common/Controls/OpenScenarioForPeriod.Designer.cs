using System.Windows.Forms;
using DateSelectionControl=Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl;

namespace Teleopti.Ccc.Win.Common.Controls
{
	partial class OpenScenarioForPeriod
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.panelSelection = new System.Windows.Forms.Panel();
			this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxScenario = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.dateSelectionControl1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl();
			this.labelScenario = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.checkBoxAdvLeaderMode = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvShrinkage = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvCalculation = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvValidation = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.panelSelection.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxScenario)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvLeaderMode)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShrinkage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvCalculation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvValidation)).BeginInit();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// panelSelection
			// 
			this.panelSelection.BackColor = System.Drawing.Color.White;
			this.panelSelection.Controls.Add(this.buttonOK);
			this.panelSelection.Controls.Add(this.buttonCancel);
			this.panelSelection.Controls.Add(this.tableLayoutPanel1);
			this.panelSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelSelection.Location = new System.Drawing.Point(177, 0);
			this.panelSelection.Margin = new System.Windows.Forms.Padding(0);
			this.panelSelection.Name = "panelSelection";
			this.panelSelection.Padding = new System.Windows.Forms.Padding(6);
			this.panelSelection.Size = new System.Drawing.Size(341, 356);
			this.panelSelection.TabIndex = 4;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOK.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.IsBackStageButton = false;
			this.buttonOK.Location = new System.Drawing.Point(149, 317);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(0);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 27);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "xxOk";
			this.buttonOK.UseVisualStyle = true;
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(244, 317);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(6);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.03279F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.96721F));
			this.tableLayoutPanel1.Controls.Add(this.comboBoxScenario, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelScenario, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(32, 14);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(303, 196);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// comboBoxScenario
			// 
			this.comboBoxScenario.BackColor = System.Drawing.Color.White;
			this.comboBoxScenario.BeforeTouchSize = new System.Drawing.Size(177, 21);
			this.comboBoxScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxScenario.FlatBorderColor = System.Drawing.SystemColors.ControlDark;
			this.comboBoxScenario.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.comboBoxScenario.Location = new System.Drawing.Point(120, 6);
			this.comboBoxScenario.Margin = new System.Windows.Forms.Padding(5, 6, 6, 6);
			this.comboBoxScenario.Name = "comboBoxScenario";
			this.comboBoxScenario.Size = new System.Drawing.Size(177, 21);
			this.comboBoxScenario.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxScenario.TabIndex = 3;
			// 
			// labelScenario
			// 
			this.labelScenario.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.labelScenario.Location = new System.Drawing.Point(3, 0);
			this.labelScenario.Name = "labelScenario";
			this.labelScenario.Size = new System.Drawing.Size(108, 30);
			this.labelScenario.TabIndex = 3;
			this.labelScenario.Text = "xxScenarioColon";
			this.labelScenario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBox1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
			this.groupBox1.Controls.Add(this.tableLayoutPanel2);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.groupBox1.Location = new System.Drawing.Point(3, 45);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(297, 148);
			this.groupBox1.TabIndex = 5;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "xxToggle";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.checkBoxAdvLeaderMode, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.checkBoxAdvShrinkage, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.checkBoxAdvCalculation, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.checkBoxAdvValidation, 0, 3);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 19);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 4;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(291, 126);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// checkBoxAdvLeaderMode
			// 
			this.checkBoxAdvLeaderMode.BeforeTouchSize = new System.Drawing.Size(285, 23);
			this.checkBoxAdvLeaderMode.DrawFocusRectangle = false;
			this.checkBoxAdvLeaderMode.Location = new System.Drawing.Point(3, 3);
			this.checkBoxAdvLeaderMode.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvLeaderMode.Name = "checkBoxAdvLeaderMode";
			this.checkBoxAdvLeaderMode.Size = new System.Drawing.Size(285, 23);
			this.checkBoxAdvLeaderMode.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvLeaderMode.TabIndex = 3;
			this.checkBoxAdvLeaderMode.Text = "xxForecasts";
			this.checkBoxAdvLeaderMode.ThemesEnabled = false;
			this.checkBoxAdvLeaderMode.CheckStateChanged += new System.EventHandler(this.checkBoxAdvLeaderModeCheckStateChanged);
			// 
			// checkBoxAdvShrinkage
			// 
			this.checkBoxAdvShrinkage.BeforeTouchSize = new System.Drawing.Size(271, 23);
			this.checkBoxAdvShrinkage.DrawFocusRectangle = false;
			this.checkBoxAdvShrinkage.Location = new System.Drawing.Point(17, 32);
			this.checkBoxAdvShrinkage.Margin = new System.Windows.Forms.Padding(17, 3, 3, 3);
			this.checkBoxAdvShrinkage.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvShrinkage.Name = "checkBoxAdvShrinkage";
			this.checkBoxAdvShrinkage.Size = new System.Drawing.Size(271, 23);
			this.checkBoxAdvShrinkage.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvShrinkage.TabIndex = 0;
			this.checkBoxAdvShrinkage.Text = "xxShrinkage";
			this.checkBoxAdvShrinkage.ThemesEnabled = false;
			this.checkBoxAdvShrinkage.CheckStateChanged += new System.EventHandler(this.checkBoxAdvShrinkageCheckStateChanged);
			// 
			// checkBoxAdvCalculation
			// 
			this.checkBoxAdvCalculation.BeforeTouchSize = new System.Drawing.Size(271, 23);
			this.checkBoxAdvCalculation.DrawFocusRectangle = false;
			this.checkBoxAdvCalculation.Location = new System.Drawing.Point(17, 61);
			this.checkBoxAdvCalculation.Margin = new System.Windows.Forms.Padding(17, 3, 3, 3);
			this.checkBoxAdvCalculation.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvCalculation.Name = "checkBoxAdvCalculation";
			this.checkBoxAdvCalculation.Size = new System.Drawing.Size(271, 23);
			this.checkBoxAdvCalculation.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvCalculation.TabIndex = 1;
			this.checkBoxAdvCalculation.Text = "xxCalculations";
			this.checkBoxAdvCalculation.ThemesEnabled = false;
			this.checkBoxAdvCalculation.CheckStateChanged += new System.EventHandler(this.checkBoxAdvCalculationCheckStateChanged);
			// 
			// checkBoxAdvValidation
			// 
			this.checkBoxAdvValidation.BeforeTouchSize = new System.Drawing.Size(285, 23);
			this.checkBoxAdvValidation.DrawFocusRectangle = false;
			this.checkBoxAdvValidation.Location = new System.Drawing.Point(3, 90);
			this.checkBoxAdvValidation.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvValidation.Name = "checkBoxAdvValidation";
			this.checkBoxAdvValidation.Size = new System.Drawing.Size(285, 23);
			this.checkBoxAdvValidation.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvValidation.TabIndex = 2;
			this.checkBoxAdvValidation.Text = "xxValidations";
			this.checkBoxAdvValidation.ThemesEnabled = false;
			this.checkBoxAdvValidation.CheckStateChanged += new System.EventHandler(this.checkBoxAdvValidationCheckStateChanged);
			//
			// dateSelectionControl1
			// 
			this.dateSelectionControl1.BackColor = System.Drawing.Color.White;
			this.dateSelectionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateSelectionControl1.Location = new System.Drawing.Point(0, 0);
			this.dateSelectionControl1.Margin = new System.Windows.Forms.Padding(0,-2,0,0);
			this.dateSelectionControl1.Name = "dateSelectionControl1";
			this.dateSelectionControl1.ShowAddButtons = false;
			this.dateSelectionControl1.ShowDateSelectionCalendar = false;
			this.dateSelectionControl1.ShowDateSelectionRolling = false;
			this.dateSelectionControl1.ShowTabArea = false;
			this.dateSelectionControl1.Size = new System.Drawing.Size(165, 298);
			this.dateSelectionControl1.TabIndex = 4;
			this.dateSelectionControl1.UseFuture = true;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.25532F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.74468F));
			this.tableLayoutPanel3.Controls.Add(this.panelSelection, 1, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(1, 0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(518, 356);
			this.tableLayoutPanel3.TabIndex = 7;
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			// 
			// OpenScenarioForPeriod
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(520, 356);
			this.Controls.Add(this.tableLayoutPanel3);
			this.tableLayoutPanel3.Controls.Add(this.dateSelectionControl1, 0, 0);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(231, 40);
			this.Name = "OpenScenarioForPeriod";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "xxOpen";
			this.panelSelection.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.comboBoxScenario)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvLeaderMode)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShrinkage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvCalculation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvValidation)).EndInit();
			this.tableLayoutPanel3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelSelection;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
		private DateSelectionControl dateSelectionControl1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		  private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxScenario;
		private System.Windows.Forms.Label labelScenario;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvShrinkage;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvCalculation;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvValidation;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvLeaderMode;
		private System.Windows.Forms.ErrorProvider errorProvider1;
	}
}