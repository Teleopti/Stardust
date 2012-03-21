using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters
{
    public class ImportForecastPresenter
    {
        private readonly ImportForecastModel _model;
        
        public ImportForecastPresenter(ImportForecastModel model)
        {
            _model = model;
        }

        public IEnumerable<IWorkload> WorkloadList { get; private set; }
        public string SkillName { get; set; }
    
        public void PopulateWorkloadList()
        {
            WorkloadList = _model.LoadWorkloadList();
        }

        public void GetSelectedSkillName()
        {
            SkillName = _model.GetSelectedSkillName();
        }

        public Guid SaveForecastFile()
        {
            return _model.SaveValidatedForecastFileInDb();
        }

        public void ValidateFile(string uploadFileName)
        {
            _model.ValidateFile(uploadFileName);
        }
    }
}
