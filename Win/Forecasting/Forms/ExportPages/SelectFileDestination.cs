﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
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
        private const string dateTimeFormat = "yyyyMMdd";
        
        public SelectFileDestination()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                setColors();
            }
        }

        private void SaveFile()
        {
            using(var saveFileDialog = new SaveFileDialog())
            {
            	saveFileDialog.Title = Resources.SelectFileDestination;
            	var period = _stateObj.ExportSkillToFileCommandModel.Period;
            	saveFileDialog.FileName = _stateObj.ExportSkillToFileCommandModel.Skill.Name + " " +
            	                           period.StartDate.Date.ToString(dateTimeFormat, CultureInfo.InvariantCulture) +
            	                           "-" +
            	                           period.EndDate.Date.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
            	saveFileDialog.Filter = Resources.CSVFile;
            	saveFileDialog.OverwritePrompt = true;
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
            	{
            		txtFileName.Text = saveFileDialog.FileName;
            	}
            }
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            txtFileName.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

        public void Populate(ExportSkillModel stateObj)
        {
            _stateObj = stateObj;
        }

        public bool Depopulate(ExportSkillModel stateObj)
        {
            if (String.IsNullOrEmpty(txtFileName.Text))
            {
                MessageBoxAdv.Show(Resources.SelectedFileDestinationDoesNotExist,
                                   Resources.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            try
            {
                var path = Path.GetDirectoryName(txtFileName.Text);
                var fileName = Path.GetFileNameWithoutExtension(txtFileName.Text);
                var fileExtension = Path.GetExtension(txtFileName.Text);
                var drives = Environment.GetLogicalDrives();

                if (path == null || fileName == null || fileExtension == null
                    || path.Length < 4
                    || drives.All(t => path.Substring(0, 3) != t)
                    || !Directory.Exists(path)
                    || path.IndexOfAny(Path.GetInvalidPathChars()) != -1
                    || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1
                    || fileExtension != ".csv")
                {
                    MessageBoxAdv.Show(Resources.SelectedFileDestinationDoesNotExist,
                                       Resources.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }


                var commandModel = stateObj.ExportSkillToFileCommandModel;
                var pathExists = txtFileName.Text.Contains("\\");

                if (pathExists)
                    commandModel.FileName = txtFileName.Text;
                else
                    commandModel.FileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" +
                                            txtFileName.Text;

                GetSelectedCheckBox(stateObj);
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    var skill = _repositoryFactory.CreateSkillRepository(uow).LoadSkill(commandModel.Skill);
                    var skillDays = _repositoryFactory.CreateSkillDayRepository(uow).FindRange(commandModel.Period,
                                                                                               skill,
                                                                                               commandModel.Scenario);
                    var exportForecastDataToFile = new ExportForecastDataToFile(skill, commandModel, skillDays);

                    exportForecastDataToFile.ExportForecastData();

                }
                return true;
            }

            catch (PathTooLongException)
            {
                MessageBoxAdv.Show(Resources.SelectedFileDestinationIsTooLong,
                                   Resources.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            catch
            {
                MessageBoxAdv.Show(Resources.CouldNotExportForecastToFile,
                                   Resources.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
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
