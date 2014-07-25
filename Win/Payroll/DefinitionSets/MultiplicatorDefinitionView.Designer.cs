namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    partial class MultiplicatorDefinitionView
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
            if (disposing)
            {
                if(components != null)
                    components.Dispose();

                if (_gridHelper != null)
                    _gridHelper.Dispose();

                OnEventMessageHandlerChanged -= (MultiplicatorDefinitionView_OnEventMessageHandlerChanged);
                UnregisterMessageBrokerEvent();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gridControlMultiplicatorDefinition = new Syncfusion.Windows.Forms.Grid.GridControl();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlMultiplicatorDefinition)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControlMultiplicatorDefinition
            // 
            this.gridControlMultiplicatorDefinition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlMultiplicatorDefinition.Location = new System.Drawing.Point(0, 0);
            this.gridControlMultiplicatorDefinition.Name = "gridControlMultiplicatorDefinition";
            this.gridControlMultiplicatorDefinition.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridControlMultiplicatorDefinition.Properties.ForceImmediateRepaint = false;
            this.gridControlMultiplicatorDefinition.Properties.MarkColHeader = false;
            this.gridControlMultiplicatorDefinition.Properties.MarkRowHeader = false;
            this.gridControlMultiplicatorDefinition.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlMultiplicatorDefinition.Size = new System.Drawing.Size(901, 324);
            this.gridControlMultiplicatorDefinition.SmartSizeBox = false;
            this.gridControlMultiplicatorDefinition.TabIndex = 3;
            this.gridControlMultiplicatorDefinition.Text = "gridControl1";
            this.gridControlMultiplicatorDefinition.UseRightToLeftCompatibleTextBox = true;
            this.gridControlMultiplicatorDefinition.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridControlMultiplicatorDefinitionKeyDown);
            // 
            // MultiplicatorDefinitionView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControlMultiplicatorDefinition);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MultiplicatorDefinitionView";
            this.Size = new System.Drawing.Size(901, 324);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlMultiplicatorDefinition)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Grid.GridControl gridControlMultiplicatorDefinition;
    }
}
