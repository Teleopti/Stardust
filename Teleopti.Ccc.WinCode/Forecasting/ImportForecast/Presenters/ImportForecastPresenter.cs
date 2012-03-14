using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting.Import;
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


        public Guid SaveForecastFile(string fileName, byte[] fileContent)
        {
            return _model.SaveForecastFileInDb(fileName, fileContent);
        }

        public IForecastFile GetForecastFile (Guid id)
        {
            return _model.GetForecastFileFromDb(id);
        }
    }
}
