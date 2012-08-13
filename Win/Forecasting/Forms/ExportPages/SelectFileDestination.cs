using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
        private readonly IRepositoryFactory _repositoryFactory = new RepositoryFactory();
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
            _saveFileDialog.FileName = _stateObj.ExportSkillToFileCommandModel.Skill.Name + " - " +
                                       _stateObj.ExportSkillToFileCommandModel.Period;
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
            var commandModel = stateObj.ExportSkillToFileCommandModel;
            commandModel.FileName = textBox1.Text;
            _saveFileDialog.Dispose();
            GetSelectedCheckBox(stateObj);
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var skill = _repositoryFactory.CreateSkillRepository(uow).LoadSkill(commandModel.Skill);
                var skillDays = _repositoryFactory.CreateSkillDayRepository(uow).FindRange(commandModel.Period, skill,
                                                                                           commandModel.Scenario);
                var command = new ForecastToFileCommand(skill, commandModel, skillDays);
                command.Execute();
            }
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

        public void GetSelectedCheckBox(ExportSkillModel stateObj)
        {
            if (stateObj != null)
            {
                if (radioButtonImportStaffing.Checked)
                    stateObj.ExportSkillToFileCommandModel.ExportType = TypeOfExport.Agents;
                if (radioButtonImportWorkload.Checked)
                    stateObj.ExportSkillToFileCommandModel.ExportType = TypeOfExport.Calls;
                if (radioButtonImportWLAndStaffing.Checked)
                    stateObj.ExportSkillToFileCommandModel.ExportType = TypeOfExport.AgentsAndCalls;
            }
        }
    }
}
