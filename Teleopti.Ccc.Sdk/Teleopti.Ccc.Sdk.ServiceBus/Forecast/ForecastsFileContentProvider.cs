using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class ForecastsFileContentProvider
    {
        private readonly IList<CsvFileRow> _fileContent;
        private readonly ICccTimeZoneInfo _timeZone;
        private readonly ICollection<IForecastsFileRow> _forecasts = new List<IForecastsFileRow>();

        public ICollection<IForecastsFileRow> Forecasts
        {
            get { return _forecasts; }
        }

        public ForecastsFileContentProvider(IList<CsvFileRow> fileContent, ICccTimeZoneInfo timeZone)
        {
            _fileContent = fileContent;
            _timeZone = timeZone;
        }

        public void LoadContent()
        {
            var validators = setupForecastsFileValidators();
            _fileContent.ForEach(row =>
                                     {
                                         if (row.Count < 6 || row.Count > 7)
                                         {
                                             throw new ValidationException(
                                                 "There are more or less columns than expected.");
                                         }
                                         for (var i = 0; i < row.Count; i++)
                                         {
                                             if (!validators[i].Validate(row[i]))
                                                 throw new ValidationException(validators[i].ErrorMessage);
                                         }
                                         _forecasts.Add(ForecastsFileRowCreator.Create(row, _timeZone));
                                     });
        }

        private static List<IForecastsFileValidator> setupForecastsFileValidators()
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
    }
}