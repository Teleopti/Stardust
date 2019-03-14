namespace SdkTestWinGui
{
	partial class EndpointTestDialog
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
			this.btnDoAction = new System.Windows.Forms.Button();
			this.pgQueryDto = new System.Windows.Forms.PropertyGrid();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tbChangesFrom = new System.Windows.Forms.TextBox();
			this.tbPage = new System.Windows.Forms.TextBox();
			this.tbChangesTo = new System.Windows.Forms.TextBox();
			this.tbPageSize = new System.Windows.Forms.TextBox();
			this.tbResponsOutput = new System.Windows.Forms.RichTextBox();
			this.cbDto = new System.Windows.Forms.ComboBox();
			this.gbExperimental = new System.Windows.Forms.GroupBox();
			this.cbEndpointName = new System.Windows.Forms.ComboBox();
			this.btnExpCall = new System.Windows.Forms.Button();
			this.cbServices = new System.Windows.Forms.ComboBox();
			this.gbExperimental.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnDoAction
			// 
			this.btnDoAction.Location = new System.Drawing.Point(3, 114);
			this.btnDoAction.Name = "btnDoAction";
			this.btnDoAction.Size = new System.Drawing.Size(303, 23);
			this.btnDoAction.TabIndex = 0;
			this.btnDoAction.Text = "Call Service";
			this.btnDoAction.UseVisualStyleBackColor = true;
			this.btnDoAction.Click += new System.EventHandler(this.btnDoAction_Click);
			// 
			// pgQueryDto
			// 
			this.pgQueryDto.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.pgQueryDto.HelpVisible = false;
			this.pgQueryDto.Location = new System.Drawing.Point(6, 100);
			this.pgQueryDto.Name = "pgQueryDto";
			this.pgQueryDto.Size = new System.Drawing.Size(297, 177);
			this.pgQueryDto.TabIndex = 2;
			this.pgQueryDto.ToolbarVisible = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "ChangesFromUTC";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 39);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(84, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "ChangesToUTC";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 65);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(32, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Page";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 91);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(52, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "PageSize";
			// 
			// tbChangesFrom
			// 
			this.tbChangesFrom.Location = new System.Drawing.Point(113, 10);
			this.tbChangesFrom.Name = "tbChangesFrom";
			this.tbChangesFrom.Size = new System.Drawing.Size(193, 20);
			this.tbChangesFrom.TabIndex = 7;
			this.tbChangesFrom.Text = "2019-01-01 12:00:00";
			// 
			// tbPage
			// 
			this.tbPage.Location = new System.Drawing.Point(113, 62);
			this.tbPage.Name = "tbPage";
			this.tbPage.Size = new System.Drawing.Size(193, 20);
			this.tbPage.TabIndex = 8;
			this.tbPage.Text = "1";
			// 
			// tbChangesTo
			// 
			this.tbChangesTo.Location = new System.Drawing.Point(113, 36);
			this.tbChangesTo.Name = "tbChangesTo";
			this.tbChangesTo.Size = new System.Drawing.Size(193, 20);
			this.tbChangesTo.TabIndex = 9;
			// 
			// tbPageSize
			// 
			this.tbPageSize.Location = new System.Drawing.Point(113, 88);
			this.tbPageSize.Name = "tbPageSize";
			this.tbPageSize.Size = new System.Drawing.Size(193, 20);
			this.tbPageSize.TabIndex = 10;
			this.tbPageSize.Text = "100";
			// 
			// tbResponsOutput
			// 
			this.tbResponsOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbResponsOutput.Location = new System.Drawing.Point(318, 8);
			this.tbResponsOutput.Name = "tbResponsOutput";
			this.tbResponsOutput.Size = new System.Drawing.Size(567, 453);
			this.tbResponsOutput.TabIndex = 11;
			this.tbResponsOutput.Text = "";
			// 
			// cbDto
			// 
			this.cbDto.FormattingEnabled = true;
			this.cbDto.Location = new System.Drawing.Point(6, 73);
			this.cbDto.Name = "cbDto";
			this.cbDto.Size = new System.Drawing.Size(297, 21);
			this.cbDto.TabIndex = 12;
			this.cbDto.SelectedIndexChanged += new System.EventHandler(this.cbDto_SelectedIndexChanged);
			// 
			// gbExperimental
			// 
			this.gbExperimental.Controls.Add(this.cbServices);
			this.gbExperimental.Controls.Add(this.btnExpCall);
			this.gbExperimental.Controls.Add(this.cbEndpointName);
			this.gbExperimental.Controls.Add(this.pgQueryDto);
			this.gbExperimental.Controls.Add(this.cbDto);
			this.gbExperimental.Location = new System.Drawing.Point(3, 149);
			this.gbExperimental.Name = "gbExperimental";
			this.gbExperimental.Size = new System.Drawing.Size(309, 312);
			this.gbExperimental.TabIndex = 13;
			this.gbExperimental.TabStop = false;
			this.gbExperimental.Text = "Experimental";
			// 
			// cbEndpointName
			// 
			this.cbEndpointName.FormattingEnabled = true;
			this.cbEndpointName.Location = new System.Drawing.Point(6, 46);
			this.cbEndpointName.Name = "cbEndpointName";
			this.cbEndpointName.Size = new System.Drawing.Size(297, 21);
			this.cbEndpointName.TabIndex = 13;
			// 
			// btnExpCall
			// 
			this.btnExpCall.Location = new System.Drawing.Point(6, 283);
			this.btnExpCall.Name = "btnExpCall";
			this.btnExpCall.Size = new System.Drawing.Size(297, 23);
			this.btnExpCall.TabIndex = 14;
			this.btnExpCall.Text = "Call Service";
			this.btnExpCall.UseVisualStyleBackColor = true;
			this.btnExpCall.Click += new System.EventHandler(this.btnExpCall_Click);
			// 
			// cbServices
			// 
			this.cbServices.FormattingEnabled = true;
			this.cbServices.Location = new System.Drawing.Point(6, 19);
			this.cbServices.Name = "cbServices";
			this.cbServices.Size = new System.Drawing.Size(297, 21);
			this.cbServices.TabIndex = 15;
			this.cbServices.SelectedIndexChanged += new System.EventHandler(this.cbServices_SelectedIndexChanged);
			// 
			// EndpointTestDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(877, 461);
			this.Controls.Add(this.gbExperimental);
			this.Controls.Add(this.tbResponsOutput);
			this.Controls.Add(this.tbPageSize);
			this.Controls.Add(this.tbChangesTo);
			this.Controls.Add(this.tbPage);
			this.Controls.Add(this.tbChangesFrom);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnDoAction);
			this.Name = "EndpointTestDialog";
			this.Text = "EndpointTestDialog";
			this.Load += new System.EventHandler(this.EndpointTestDialog_Load);
			this.gbExperimental.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnDoAction;
		private System.Windows.Forms.PropertyGrid pgQueryDto;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbChangesFrom;
		private System.Windows.Forms.TextBox tbPage;
		private System.Windows.Forms.TextBox tbChangesTo;
		private System.Windows.Forms.TextBox tbPageSize;
		private System.Windows.Forms.RichTextBox tbResponsOutput;
		private System.Windows.Forms.ComboBox cbDto;
		private System.Windows.Forms.GroupBox gbExperimental;
		private System.Windows.Forms.Button btnExpCall;
		private System.Windows.Forms.ComboBox cbEndpointName;
		private System.Windows.Forms.ComboBox cbServices;
	}
}