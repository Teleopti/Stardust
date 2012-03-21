using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class ForecastsFileContentProvider
    {
        private readonly ICollection<IForecastsFileRow> _forecasts = new List<IForecastsFileRow>();

        public ICollection<IForecastsFileRow> Forecasts
        {
            get { return _forecasts; }
        }

        public ForecastsFileContentProvider(byte[] fileContent, ICccTimeZoneInfo timeZone)
        {
            loadContent(fileContent, timeZone);
        }

        private void loadContent(byte[] fileContent, ICccTimeZoneInfo timeZone)
        {
            var validators = setupForecastsFileValidators();
            var rows = fileContent.ToString().Split('\n').Select(line => new CsvFileRow(line)).ToList();
            rows.ForEach(row =>
            {
                if (!ForecastsFileRowCreator.IsFileColumnValid(row))
                {
                    throw new ValidationException("There are more or less columns than expected.");
                }
                for (var i = 0; i < row.Count; i++)
                {
                    if (!validators[i].Validate(row.Content[i]))
                        throw new ValidationException(validators[i].ErrorMessage);
                }
                _forecasts.Add(ForecastsFileRowCreator.Create(row, timeZone));
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

        public ForecastsAnalyzeCommandResult Analyze()
        {
            var analyzeCommand = new ForecastsAnalyzeCommand(Forecasts);
            return analyzeCommand.Execute();
        }
    }
}