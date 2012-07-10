using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
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
        private SaveFileDialog _saveFileDialog; 

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
            _saveFileDialog = new SaveFileDialog();
            _saveFileDialog.Title = Resources.SelectFileDestination;
            _saveFileDialog.FileName = _stateObj.ExportSkillToFileCommandModel.Period.ToString();
            _saveFileDialog.Filter = Resources.CSVFile;

            if (_saveFileDialog.ShowDialog() == DialogResult.OK) textBox1.Text = _saveFileDialog.FileName;
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
            if (String.IsNullOrEmpty(textBox1.Text)) return false;
            stateObj.ExportSkillToFileCommandModel.FileName = textBox1.Text;
            _saveFileDialog.Dispose();

            

            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return Resources.SelectFileDestination; }
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
