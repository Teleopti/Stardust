using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Helper;
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
            _presenter = new ImportForecastPresenter(this, new ImportForecastModel(preselectedSkill, unitOfWorkFactory, importForecastsRepository));
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
            set { radioButtonImportWorkload.Checked = value;}
        }

        public bool IsStaffingImport
        {
            get { return radioButtonImportStaffing.Checked; }
            set { radioButtonImportStaffing.Checked = value; }
        }

        public bool IsStaffingAndWorkloadImport
        {
            get { return radioButtonImportWLAndStaffing.Checked; }
            set { radioButtonImportWLAndStaffing.Checked = value; }
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
            var fileContent = new List<CsvFileRow>();
            using (var stream = new StreamReader(textBoxImportFileName.Text))
            {
                var reader = new CsvFileReader(stream);
                var row = new CsvFileRow();
                var rowNumber = 1;
                var validators = setUpForecastsFileValidators();
                try
                {
                    using (PerformanceOutput.ForOperation("Validate forecasts import file."))
                    {
                        while (reader.ReadNextRow(row))
                        {
                            validateRowByRow(validators, row);
                            var rowToBeSave = new CsvFileRow();
                            rowToBeSave.AddRange(row);
                            fileContent.Add(rowToBeSave);
                            row.Clear();
                            rowNumber++;
                        }
                    }
                    MessageBoxAdv.Show("Validation succeeded.");
                }
                catch (ValidationException exception)
                {
                    MessageBoxAdv.Show(string.Format("LineNumber{0}, Error:{1}", rowNumber, exception.Message),"ValidationError");
                }
                var savedFileId = saveFileToServer(fileContent);
                if (savedFileId == null)
                {
                    MessageBoxAdv.Show("Error occured when trying to import file.");
                    return;
                }
                var dto = new ImportForecastsFileCommandDto
                {
                    ImportForecastsMode = getImportForecastOption(),
                    UploadedFileId = savedFileId.GetValueOrDefault(),
                    TargetSkillId = _skill.Id.GetValueOrDefault()
                };
                var statusDialog = new JobStatusView(new JobStatusModel { JobStatusId = Guid.Empty, CommandDto = dto });
                statusDialog.Show(this);
                statusDialog.SetJobStatusId(executeCommand(dto));
            }
        }

        private static void validateRowByRow(IList<IForecastsFileValidator> validators, IList<string> row)
        {
            if (row.Count < 6 || row.Count > 7)
            {
                throw new ValidationException("There are more or less columns than expected.");
            }
            for (var i = 0; i < row.Count; i++)
            {
                if (!validators[i].Validate(row[i]))
                    throw new ValidationException(validators[i].ErrorMessage);
            }
        }

        private Guid? saveFileToServer(IList<CsvFileRow> fileContent)
        {
            Guid? savedFileId = null;
            _gracefulHandler.AttemptDatabaseConnectionDependentAction(
                () => {
                        IFormatter formatter = new BinaryFormatter();
                        byte[] serializedValue;
                        using (var stream = new MemoryStream())
                        {
                            formatter.Serialize(stream, fileContent);
                            serializedValue = stream.GetBuffer();
                        }
                        savedFileId = _presenter.SaveForecastFile(textBoxImportFileName.Text, serializedValue);
                    });
            return savedFileId;
        }

        private static List<IForecastsFileValidator> setUpForecastsFileValidators()
        {
            var validators = new List<IForecastsFileValidator>
                                 {
                                     new ForecastsFileSkillNameValidator(),
                                     new ForecastsFileDateTimeValidator(),
                                     new ForecastsFileDateTimeValidator(),
                                     new ForecastsFileIntegerValueValidator(),
                                     new ForecastsFileDoubleValueValidator(),
                                     new ForecastsFileDoubleValueValidator(),
                                     new ForecastsFileDoubleValueValidator()
                                 };
            return validators;
        }

        private Guid? executeCommand(CommandDto commandDto)
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
                Logger.Error("Import forecasts command can't reach Sdk due to a timeout.", timeoutException);
            }
            catch (CommunicationException exception)
            {
                Logger.Error("Import forecasts command can't reach Sdk.", exception);
            }
            catch (Exception exception)
            {
                Logger.Error("Import forecasts command notification error.", exception);
            }
            finally
            {
                ((IDisposable)proxy).Dispose();
            }
            return null;
        }
    }
}
