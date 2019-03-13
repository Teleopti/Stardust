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
			this.tbResponsOutput = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnDoAction
			// 
			this.btnDoAction.Location = new System.Drawing.Point(12, 10);
			this.btnDoAction.Name = "btnDoAction";
			this.btnDoAction.Size = new System.Drawing.Size(75, 23);
			this.btnDoAction.TabIndex = 0;
			this.btnDoAction.Text = "Call";
			this.btnDoAction.UseVisualStyleBackColor = true;
			this.btnDoAction.Click += new System.EventHandler(this.btnDoAction_Click);
			// 
			// tbResponsOutput
			// 
			this.tbResponsOutput.Location = new System.Drawing.Point(12, 39);
			this.tbResponsOutput.Multiline = true;
			this.tbResponsOutput.Name = "tbResponsOutput";
			this.tbResponsOutput.Size = new System.Drawing.Size(776, 399);
			this.tbResponsOutput.TabIndex = 1;
			// 
			// EndpointTestDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.tbResponsOutput);
			this.Controls.Add(this.btnDoAction);
			this.Name = "EndpointTestDialog";
			this.Text = "EndpointTestDialog";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnDoAction;
		private System.Windows.Forms.TextBox tbResponsOutput;
	}
}