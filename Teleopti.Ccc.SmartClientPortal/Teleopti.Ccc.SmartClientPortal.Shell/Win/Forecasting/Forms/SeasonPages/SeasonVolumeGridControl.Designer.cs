namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SeasonPages
{
    partial class SeasonVolumeGridControl
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
                if (components != null)
                    components.Dispose();
                _owner = null;
                _volumeYear = null;
                _skillType = null;
                if (_gridRows!=null)
                {
                    _gridRows.Clear();
                }
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
            this.SuspendLayout();
            // 
            // VolumeGridControl
            // 
            this.Name = "VolumeGridControl";
            this.ResumeLayout(false);

        }

        #endregion
    }
}