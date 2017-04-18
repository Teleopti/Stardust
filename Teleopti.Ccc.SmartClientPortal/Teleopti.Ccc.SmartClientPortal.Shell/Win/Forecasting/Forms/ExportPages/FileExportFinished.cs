using System.Collections.Generic;
using System.IO;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ExportPages
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
