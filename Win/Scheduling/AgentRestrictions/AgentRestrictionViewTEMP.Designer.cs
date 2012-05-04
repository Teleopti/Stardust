namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	partial class AgentRestrictionViewTemp
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "agentRestrictionGrid"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AgentRestrictionViewTEMP"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button1 = new System.Windows.Forms.Button();
			this.agentRestrictionGrid = new Teleopti.Ccc.Win.Scheduling.AgentRestrictions.AgentRestrictionGrid(this.components);
			((System.ComponentModel.ISupportInitialize)(this.agentRestrictionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(689, 386);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 5;
			this.button1.Text = "Close";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// agentRestrictionGrid
			// 
			this.agentRestrictionGrid.ExcelLikeCurrentCell = true;
			this.agentRestrictionGrid.ExcelLikeSelectionFrame = true;
			this.agentRestrictionGrid.GridLineColor = System.Drawing.SystemColors.GrayText;
			this.agentRestrictionGrid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.agentRestrictionGrid.Location = new System.Drawing.Point(12, 12);
			this.agentRestrictionGrid.Name = "agentRestrictionGrid";
			this.agentRestrictionGrid.NumberedColHeaders = false;
			this.agentRestrictionGrid.NumberedRowHeaders = false;
			this.agentRestrictionGrid.ReadOnly = true;
			this.agentRestrictionGrid.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.agentRestrictionGrid.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.AlwaysVisible;
			this.agentRestrictionGrid.Size = new System.Drawing.Size(752, 212);
			this.agentRestrictionGrid.SmartSizeBox = false;
			this.agentRestrictionGrid.TabIndex = 6;
			this.agentRestrictionGrid.Text = "agentRestrictionGrid";
			this.agentRestrictionGrid.ThemesEnabled = true;
			this.agentRestrictionGrid.UseRightToLeftCompatibleTextBox = true;
			// 
			// AgentRestrictionViewTemp
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(776, 421);
			this.Controls.Add(this.agentRestrictionGrid);
			this.Controls.Add(this.button1);
			this.Name = "AgentRestrictionViewTemp";
			this.Text = "AgentRestrictionViewTEMP";
			((System.ComponentModel.ISupportInitialize)(this.agentRestrictionGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private AgentRestrictionGrid agentRestrictionGrid;
	}
}