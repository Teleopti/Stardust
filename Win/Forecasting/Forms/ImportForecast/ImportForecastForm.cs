using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Forecasting.Forms.ExportPages;
using Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;
using log4net;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ImportForecast
{
    public partial class ImportForecastForm : BaseRibbonForm, IImportForecast
    {
        private readonly ImportForecastPresenter _presenter;
        private ISkill _skill;
        private readonly IGracefulDataSourceExceptionHandler _gracefulHandler;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportForecastForm));
        
        public ImportForecastForm(ISkill preselectedSkill, IUnitOfWorkFactory unitOfWorkFactory, IImportForecastsRepository importForecastsRepository, IGracefulDataSourceExceptionHandler gracefulHandler)
        {
            InitializeComponent();
            _presenter = new ImportForecastPresenter(new ImportForecastModel(preselectedSkill, unitOfWorkFactory, importForecastsRepository));
            getSelectedSkillName();
            populateWorkloadList();
            _skill = preselectedSkill;
            _gracefulHandler = gracefulHandler;
        }

        private void populateWorkloadList()
        {
            _presenter.PopulateWorkloadList();
            var firstWorkload = _presenter.WorkloadList.FirstOrDefault();
            if(firstWorkload==null)
            {
                throw new ArgumentNullException("No workload exists.");
            }
            labelWorkloadName.Text = firstWorkload.Name;
        }

        private void getSelectedSkillName()
        {
            _presenter.GetSelectedSkillName();
            txtSkillName.Text = _presenter.SkillName;
        }

        private void browseImportFileButtonClick(object sender, EventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            
            if (result == DialogResult.OK)
            {
                textBoxImportFileName.Text = openFileDialog.FileName;
            }
        }

        public ISkill Skill
        {
            get { return _skill; }
            set { _skill = value; }
        }

        public bool IsWorkloadImport
        {
            get { return radioButtonImportWorkload.Checked; }
        }

        public bool IsStaffingImport
        {
            get { return radioButtonImportStaffing.Checked; }
        }

        public bool IsStaffingAndWorkloadImport
        {
            get { return radioButtonImportWLAndStaffing.Checked; }
        }

        private ImportForecastsOptionsDto getImportForecastOption()
        {
            if (IsWorkloadImport)
                return ImportForecastsOptionsDto.ImportWorkload;
            if (IsStaffingImport)
                return ImportForecastsOptionsDto.ImportStaffing;
            if (IsStaffingAndWorkloadImport)
                return ImportForecastsOptionsDto.ImportWorkloadAndStaffing;
            throw new NotSupportedException("Options not supported.");
        }

        private void buttonAdvImportClick(object sender, EventArgs e)
        {
            buttonAdvImport.Enabled = false;

            _presenter.ValidateFile(UploadFileName);
            var savedFileId = saveFileToServer();
            var statusDialog = new JobStatusView(new JobStatusModel {JobStatusId = Guid.NewGuid()});
            statusDialog.Show(this);
            statusDialog.SetProgress(1);
            statusDialog.SetMessage("Uploading file:" + UploadFileName + " to server...");
            if (savedFileId == Guid.Empty)
            {
                MessageBoxAdv.Show("Error occured when trying to import file.");
                return;
            }
            statusDialog.SetJobStatusId(savedFileId);
            statusDialog.SetProgress(2);
            statusDialog.SetMessage(UploadFileName + " uploaded.");
            statusDialog.SetProgress(1);
            statusDialog.SetMessage("Validating...");
            var dto = new ImportForecastsFileCommandDto
                          {
                              ImportForecastsMode = getImportForecastOption(),
                              UploadedFileId = savedFileId,
                              TargetSkillId = _skill.Id.GetValueOrDefault()
                          };
            statusDialog.SetJobStatusId(executeCommand(dto));
        }

        private string UploadFileName
        {
            get { return textBoxImportFileName.Text; }
        }

        private Guid saveFileToServer()
        {
            var savedFileId = Guid.Empty;
            _gracefulHandler.AttemptDatabaseConnectionDependentAction(() =>
            {
                savedFileId = _presenter.SaveForecastFile();
            });
            return savedFileId;
        }

        private static Guid executeCommand(CommandDto commandDto)
        {
            var sdkAuthentication = new SdkAuthentication();
            sdkAuthentication.SetSdkAuthenticationHeader();

            var proxy = new Proxy();
            try
            {
                proxy.Open();
                var result = proxy.ExecuteCommand(commandDto);
                return result.AffectedId.GetValueOrDefault();
            }
            catch (TimeoutException timeoutException)
            {
                Logger.Error(string.Concat(commandDto.GetType(), " can't reach Sdk due to a timeout."), timeoutException);
            }
            catch (CommunicationException exception)
            {
                Logger.Error(string.Concat(commandDto.GetType(), " can't reach Sdk."), exception);
            }
            catch (Exception exception)
            {
                Logger.Error(string.Concat(commandDto.GetType(), " notification error."), exception);
            }
            finally
            {
                ((IDisposable)proxy).Dispose();
            }
            return Guid.Empty;
        }

        private void textBoxImportFileNameTextChanged(object sender, EventArgs e)
        {
            if (File.Exists(UploadFileName))
                buttonAdvImport.Enabled = true;
        }
    }
}
