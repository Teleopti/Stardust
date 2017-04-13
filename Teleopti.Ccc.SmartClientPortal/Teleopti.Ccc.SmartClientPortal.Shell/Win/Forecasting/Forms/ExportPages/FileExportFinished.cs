using System.Collections.Generic;
using System.IO;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    public partial class FileExportFinished : BaseUserControl, IPropertyPageNoRoot<ExportSkillModel>
    {
        private readonly ICollection<string> _errorMessages = new List<string>();

        public FileExportFinished()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                setColors();
            }
                
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
        }

        public void Populate(ExportSkillModel stateObj)
        {
            label1.Text = File.Exists(stateObj.ExportSkillToFileCommandModel.FileName) ? Resources.FileExportDone : Resources.FileExportFailed;
        }

        public bool Depopulate(ExportSkillModel stateObj)
        {
            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return Resources.FileExportFinished; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }
    }
}
