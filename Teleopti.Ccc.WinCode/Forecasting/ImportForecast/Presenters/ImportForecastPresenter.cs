using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters
{
    public class ImportForecastPresenter
    {
        private readonly IImportForecastView _view;
        private readonly ImportForecastModel _model;
        private readonly ISaveImportForecastFileCommand _saveImportForecastFileCommand;
        private readonly IValidateImportForecastFileCommand _validateImportForecastFileCommand;
        private readonly ISendCommandToSdk _sendCommandToSdk;

        public ImportForecastPresenter(IImportForecastView view, ImportForecastModel model, ISaveImportForecastFileCommand saveImportForecastFileCommand, IValidateImportForecastFileCommand validateImportForecastFileCommand, ISendCommandToSdk sendCommandToSdk)
        {
            _view = view;
            _model = model;
            _saveImportForecastFileCommand = saveImportForecastFileCommand;
            _validateImportForecastFileCommand = validateImportForecastFileCommand;
            _sendCommandToSdk = sendCommandToSdk;
        }

        public void Initialize()
        {
            var skill = _model.SelectedSkill;
            _view.SetSkillName(skill.Name);

            var workload = _model.SelectedWorkload();
            if(workload==null)
                throw new InvalidOperationException("Workload should not be null.");
            _view.SetWorkloadName(workload.Name);
            _view.SetVisibility(skill.SkillType);
        }

        public void SetImportType(ImportForecastsMode importMode)
        {
            _model.ImportMode = importMode;
        }

        public void StartImport(string fileName)
        {
            _validateImportForecastFileCommand.Execute(fileName);
            if (_model.HasValidationError)
            {
                _view.ShowValidationException(_model.ValidationMessage);
                return;
            }

            _saveImportForecastFileCommand.Execute(fileName);

            if (_model.FileId == Guid.Empty)
            {
                _view.ShowError("Error occured when trying to import file.");
                return;
            }
            var dto = new ImportForecastsFileCommandDto
                          {
                              ImportForecastsMode = (ImportForecastsOptionsDto)(int)_model.ImportMode,
                              UploadedFileId = _model.FileId,
                              TargetSkillId = _model.SelectedSkill.Id.GetValueOrDefault()
                          };

            _view.ShowStatusDialog(_sendCommandToSdk.ExecuteCommand(dto).AffectedId.GetValueOrDefault());
        }
    }
}
