using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters
{
    public class SaveImportForecastFileCommand : ISaveImportForecastFileCommand
    {
        private readonly ImportForecastModel _model;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IImportForecastsRepository _importForecastsRepository;

        public SaveImportForecastFileCommand(ImportForecastModel model, IUnitOfWorkFactory unitOfWorkFactory, IImportForecastsRepository importForecastsRepository)
        {
            _model = model;
            _unitOfWorkFactory = unitOfWorkFactory;
            _importForecastsRepository = importForecastsRepository;
        }

        public void Execute(string fileName)
        {
            _model.FileId = saveForecastFile(fileName);
        }

        
        private Guid saveForecastFile(string uploadFileName)
        {
            Guid result;
            var forecastFile = new ForecastFile(uploadFileName, _model.FileContent);
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _importForecastsRepository.Add(forecastFile);
                uow.PersistAll();
                result = forecastFile.Id.GetValueOrDefault();
            }
            return result;
        }
    }
}