﻿using DateSelectionControl=Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenScenarioForPeriod));
            this.panelSelection = new System.Windows.Forms.Panel();
            this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxScenario = new System.Windows.Forms.ComboBox();
            this.labelScenario = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxAdvLeaderMode = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.checkBoxAdvShrinkage = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.checkBoxAdvCalculation = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.checkBoxAdvValidation = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.dateSelectionControl1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.panelSelection.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvLeaderMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShrinkage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvCalculation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvValidation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
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
            this.panelSelection.Location = new System.Drawing.Point(165, 0);
            this.panelSelection.Margin = new System.Windows.Forms.Padding(0);
            this.panelSelection.Name = "panelSelection";
            this.panelSelection.Padding = new System.Windows.Forms.Padding(5);
            this.panelSelection.Size = new System.Drawing.Size(318, 298);
            this.panelSelection.TabIndex = 4;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(165, 266);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(0);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(69, 22);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "xxOk";
            this.buttonOK.UseVisualStyle = true;
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(239, 266);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(5);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(69, 22);
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(291, 170);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // comboBoxScenario
            // 
            this.comboBoxScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxScenario.FormattingEnabled = true;
            this.comboBoxScenario.Location = new System.Drawing.Point(114, 5);
            this.comboBoxScenario.Margin = new System.Windows.Forms.Padding(4, 5, 5, 5);
            this.comboBoxScenario.Name = "comboBoxScenario";
            this.comboBoxScenario.Size = new System.Drawing.Size(155, 21);
            this.comboBoxScenario.TabIndex = 3;
            // 
            // labelScenario
            // 
            this.labelScenario.Location = new System.Drawing.Point(3, 0);
            this.labelScenario.Name = "labelScenario";
            this.labelScenario.Size = new System.Drawing.Size(93, 26);
            this.labelScenario.TabIndex = 3;
            this.labelScenario.Text = "xxScenarioColon";
            this.labelScenario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(3, 41);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(282, 126);
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
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(276, 107);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // checkBoxAdvLeaderMode
            // 
            this.checkBoxAdvLeaderMode.Location = new System.Drawing.Point(3, 3);
            this.checkBoxAdvLeaderMode.Name = "checkBoxAdvLeaderMode";
            this.checkBoxAdvLeaderMode.Size = new System.Drawing.Size(270, 20);
            this.checkBoxAdvLeaderMode.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvLeaderMode.TabIndex = 3;
            this.checkBoxAdvLeaderMode.Text = "xxForecasts";
            this.checkBoxAdvLeaderMode.ThemesEnabled = false;
            this.checkBoxAdvLeaderMode.CheckStateChanged += new System.EventHandler(this.checkBoxAdvLeaderModeCheckStateChanged);
            // 
            // checkBoxAdvShrinkage
            // 
            this.checkBoxAdvShrinkage.Location = new System.Drawing.Point(15, 29);
            this.checkBoxAdvShrinkage.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.checkBoxAdvShrinkage.Name = "checkBoxAdvShrinkage";
            this.checkBoxAdvShrinkage.Size = new System.Drawing.Size(258, 20);
            this.checkBoxAdvShrinkage.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvShrinkage.TabIndex = 0;
            this.checkBoxAdvShrinkage.Text = "xxShrinkage";
            this.checkBoxAdvShrinkage.ThemesEnabled = false;
            this.checkBoxAdvShrinkage.CheckStateChanged += new System.EventHandler(this.checkBoxAdvShrinkageCheckStateChanged);
            // 
            // checkBoxAdvCalculation
            // 
            this.checkBoxAdvCalculation.Location = new System.Drawing.Point(15, 55);
            this.checkBoxAdvCalculation.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.checkBoxAdvCalculation.Name = "checkBoxAdvCalculation";
            this.checkBoxAdvCalculation.Size = new System.Drawing.Size(258, 20);
            this.checkBoxAdvCalculation.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvCalculation.TabIndex = 1;
            this.checkBoxAdvCalculation.Text = "xxCalculations";
            this.checkBoxAdvCalculation.ThemesEnabled = false;
            this.checkBoxAdvCalculation.CheckStateChanged += new System.EventHandler(this.checkBoxAdvCalculationCheckStateChanged);
            // 
            // checkBoxAdvValidation
            // 
            this.checkBoxAdvValidation.Location = new System.Drawing.Point(3, 81);
            this.checkBoxAdvValidation.Name = "checkBoxAdvValidation";
            this.checkBoxAdvValidation.Size = new System.Drawing.Size(270, 20);
            this.checkBoxAdvValidation.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvValidation.TabIndex = 2;
            this.checkBoxAdvValidation.Text = "xxValidations";
            this.checkBoxAdvValidation.ThemesEnabled = false;
            this.checkBoxAdvValidation.CheckStateChanged += new System.EventHandler(this.checkBoxAdvValidationCheckStateChanged);
            // 
            // dateSelectionControl1
            // 
            this.dateSelectionControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(228)))), ((int)(((byte)(246)))));
            this.dateSelectionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateSelectionControl1.Location = new System.Drawing.Point(0, 0);
            this.dateSelectionControl1.Margin = new System.Windows.Forms.Padding(0);
            this.dateSelectionControl1.Name = "dateSelectionControl1";
            this.dateSelectionControl1.ShowAddButtons = false;
            this.dateSelectionControl1.ShowDateSelectionCalendar = false;
            this.dateSelectionControl1.ShowDateSelectionRolling = false;
            this.dateSelectionControl1.ShowTabArea = true;
            this.dateSelectionControl1.Size = new System.Drawing.Size(165, 298);
            this.dateSelectionControl1.TabIndex = 4;
            this.dateSelectionControl1.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.dateSelectionControl1.TabPanelBorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dateSelectionControl1.UseFuture = true;
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonText = "";
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.SelectedTab = null;
            this.ribbonControlAdv1.ShowMinimizeButton = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(493, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "xxStart menu";
            this.ribbonControlAdv1.TabIndex = 6;
            this.ribbonControlAdv1.Text = "xxOpen";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.25532F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.74468F));
            this.tableLayoutPanel3.Controls.Add(this.dateSelectionControl1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.panelSelection, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(483, 298);
            this.tableLayoutPanel3.TabIndex = 7;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // OpenScenarioForPeriod
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(495, 338);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this.ribbonControlAdv1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(200, 40);
            this.Name = "OpenScenarioForPeriod";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "xxOpen";
            this.panelSelection.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvLeaderMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShrinkage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvCalculation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvValidation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSelection;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private DateSelectionControl dateSelectionControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox comboBoxScenario;
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