using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters
{
    public class ImportForecastPresenter
    {
        private readonly IImportForecast _view;
        private readonly ImportForecastModel _model;
        
        public ImportForecastPresenter(IImportForecast view, ImportForecastModel model)
        {
            _view = view;
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


        public void SaveForecastFile(string fileName, byte[] fileContent)
        {
            _model.SaveForecastFileInDb(fileName, fileContent);
        }
    }
}
