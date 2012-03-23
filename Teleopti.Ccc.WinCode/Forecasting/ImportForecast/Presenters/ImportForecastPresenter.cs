using System;
using System.IO;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters
{
    public class ImportForecastPresenter
    {
        private readonly IImportForecastView _view;
        private readonly IImportForecastModel _model;

        public ImportForecastPresenter(IImportForecastView view, IImportForecastModel model)
        {
            _view = view;
            _model = model;
        }

        public string SkillName { get; set; }

        public IWorkload Workload { get; set; }

        public void GetSelectedSkillName()
        {
            SkillName = _model.GetSelectedSkillName();
        }

        public Guid SaveForecastFile(string uploadFileName)
        {
            return _model.SaveValidatedForecastFileInDb(uploadFileName);
        }

        public void ValidateFile(string uploadFileName)
        {
            using (var stream = new StreamReader(uploadFileName))
            {
                _model.ValidateFile(stream);
            }
        }

        public ImportForecastsOptionsDto GetImportForecastOption()
        {
            if (_view.IsWorkloadImport)
                return ImportForecastsOptionsDto.ImportWorkload;
            if (_view.IsStaffingImport)
                return ImportForecastsOptionsDto.ImportStaffing;
            if (_view.IsStaffingAndWorkloadImport)
                return ImportForecastsOptionsDto.ImportWorkloadAndStaffing;
            throw new NotSupportedException("Options not supported.");
        }

        public void PopulateWorkload()
        {
            var workload = _model.LoadWorkload();
            if(workload==null)
                throw new InvalidOperationException("Workload should not be null.");
            Workload = workload;
        }
    }
}
