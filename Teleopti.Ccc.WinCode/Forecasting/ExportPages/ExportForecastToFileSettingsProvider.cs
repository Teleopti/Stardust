﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public class ExportForecastToFileSettingsProvider : IExportForecastToFileSettingsProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private IExportForecastToFileSettings _settings;

        public ExportForecastToFileSettingsProvider(IUnitOfWorkFactory unitOfWorkFactory,
                                                         IRepositoryFactory repositoryFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
        }
        
        
        public IExportForecastToFileSettings ExportForecastToFileSettings
        {
            get
            {
                if (_settings == null)
                {
                    using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        _settings = _repositoryFactory.CreateGlobalSettingDataRepository(uow).FindValueByKey
                            <IExportForecastToFileSettings>(
                                "ExportForecastToFileSettings", new ExportForecastToFileSettings());
                    }
                }
                return _settings;
            }
        }

        public void Save()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _repositoryFactory.CreateGlobalSettingDataRepository(uow).PersistSettingValue(ExportForecastToFileSettings);
                uow.PersistAll();
            }
        }

        public void TransformToSerilizableModel(DateOnlyPeriod period)
        {
            ExportForecastToFileSettings.Period = period;
        }
    }
}
