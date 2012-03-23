using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public interface IForecastsFileContentProvider
    {
        ICollection<IForecastsFileRow> Forecasts { get; }
        IForecastsFileContentProvider LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone);
        IForecastsAnalyzeCommandResult Analyze();
    }

    public class ForecastsFileContentProvider : IForecastsFileContentProvider
    {
        private readonly ICollection<IForecastsFileRow> _forecasts = new List<IForecastsFileRow>();
    	private static readonly IList<IForecastsFileValidator> _validators = new List<IForecastsFileValidator>
    	                                                                  	{
    	                                                                  		new ForecastsFileSkillNameValidator(),
    	                                                                  		new ForecastsFileDateTimeValidator(),
    	                                                                  		new ForecastsFileDateTimeValidator(),
    	                                                                  		new ForecastsFileIntegerValueValidator(),
    	                                                                  		new ForecastsFileDoubleValueValidator(),
    	                                                                  		new ForecastsFileDoubleValueValidator(),
    	                                                                  		new ForecastsFileDoubleValueValidator()
    	                                                                  	};

        public ICollection<IForecastsFileRow> Forecasts
        {
            get { return _forecasts; }
        }
        
        public IForecastsFileContentProvider LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone)
        {
            var rows = Encoding.UTF8.GetString(fileContent).Split('\n').Select(line => new CsvFileRow(line)).ToList();
            rows.ForEach(row =>
            {
                if (!ForecastsFileRowCreator.IsFileColumnValid(row))
                {
                    throw new ValidationException("There are more or less columns than expected.");
                }
                for (var i = 0; i < row.Count; i++)
                {
                    if (!_validators[i].Validate(row.Content[i]))
                        throw new ValidationException(_validators[i].ErrorMessage);
                }
                Forecasts.Add(ForecastsFileRowCreator.Create(row, timeZone));
            });
            return this;
        }

        public IForecastsAnalyzeCommandResult Analyze()
        {
            if(Forecasts.Count == 0)
                throw new InvalidOperationException("Forecasts should be not empty.");
            var analyzeCommand = new ForecastsAnalyzeCommand(Forecasts);
            return analyzeCommand.Execute();
        }
    }
}