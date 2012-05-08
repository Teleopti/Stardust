using System.Drawing;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.SpinningProgress
{
    partial class SpinningProgressControl : System.Windows.Forms.UserControl 
    { 
        
        // UserControl1 overrides dispose to clean up the component list.
        [ System.Diagnostics.DebuggerNonUserCode() ]
        protected override void Dispose( bool disposing ) 
        { 
            innerBackgroundRegion.Dispose();
            if ( disposing && components != null ) 
            { 
                components.Dispose(); 
            } 
            base.Dispose( disposing ); 
        } 
        
        
        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components = null; 
        
        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [ System.Diagnostics.DebuggerStepThrough() ]
        private void InitializeComponent() 
        { 
            this.SuspendLayout(); 
            // 
            // SpinningProgress
            // 
            this.Name = "SpinningProgress"; 
            this.MinimumSize = new Size(20, 20);
            this.Size = new System.Drawing.Size( 20, 20 ); 
            this.ResumeLayout( false ); 
            
        } 
        
        
    }
}