using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;


namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    public partial class SelectFileDestination : BaseUserControl, IPropertyPageNoRoot<ExportSkillModel>
    {
        private ExportSkillModel _stateObj;
        private readonly ICollection<string> _errorMessages = new List<string>();

        public SelectFileDestination()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                setColors();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SaveFile();
        }

        private void SaveFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            // Default mapp?
            saveFileDialog.InitialDirectory = @"C:\";
            // Resources
            saveFileDialog.Title = "Select destination of file";
            // Hämta default filnamn?
            saveFileDialog.DefaultExt = "Default";
            // Resources ?
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.OK) textBox1.Text = saveFileDialog.FileName;
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            textBox1.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

        public void Populate(ExportSkillModel stateObj)
        {
            _stateObj = stateObj;
        }

        public bool Depopulate(ExportSkillModel stateObj)
        {
            if (textBox1.Text == "") return false;
            stateObj.ExportSkillToFileCommandModel.FileName = textBox1.Text;
            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            // Ska ändras 
            get { return "File Destination: Name not Implemented yet"; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }

        private void btnChoose_Click(object sender, EventArgs e)
        {
            SaveFile();
        }
    }
}
