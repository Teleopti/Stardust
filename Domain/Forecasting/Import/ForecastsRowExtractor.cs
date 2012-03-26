using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastsRowExtractor
    {
        IForecastsFileRow Extract(string rowString, ICccTimeZoneInfo timeZone);
    }

    public class ForecastsRowExtractor : IForecastsRowExtractor
    {
        private readonly ISpecification<string[]> _columnsInRowValidSpecification = new IsColumnCountInRowValid();
        private readonly ForecastsFileSkillNameValidator _skillNameValidator = new ForecastsFileSkillNameValidator();
        private readonly ForecastsFileDateTimeValidator _dateTimeValidator = new ForecastsFileDateTimeValidator();
        private readonly ForecastsFileIntegerValueValidator _integerValidator = new ForecastsFileIntegerValueValidator();
        private readonly ForecastsFileDoubleValueValidator _doubleValidator = new ForecastsFileDoubleValueValidator();

        public IForecastsFileRow Extract(string rowString, ICccTimeZoneInfo timeZone)
        {
            var content = rowString.Split(',');
            if (!_columnsInRowValidSpecification.IsSatisfiedBy(content))
            {
                throw new ValidationException("There are more or less columns than expected.");
            }
            var newRow = new ForecastsFileRow();

            ForecastParseResult<string> stringResult;
            if (!_skillNameValidator.TryParse(content[0], out stringResult))
            {
                throw new ValidationException(stringResult.ErrorMessage);
            }
            newRow.SkillName = stringResult.Value;

            ForecastParseResult<DateTime> dateTimeResult;
            if (!_dateTimeValidator.TryParse(content[1], out dateTimeResult))
            {
                throw new ValidationException(dateTimeResult.ErrorMessage);
            }
            newRow.LocalDateTimeFrom = dateTimeResult.Value;
            if (timeZone.IsInvalidTime(newRow.LocalDateTimeFrom))
            {
                throw new ValidationException(string.Format("{0} is invalid time.", newRow.LocalDateTimeFrom));
            }
            
            if (!_dateTimeValidator.TryParse(content[2], out dateTimeResult))
            {
                throw new ValidationException(dateTimeResult.ErrorMessage);
            }
            newRow.LocalDateTimeTo = dateTimeResult.Value;
            if (timeZone.IsInvalidTime(newRow.LocalDateTimeTo))
            {
                throw new ValidationException(string.Format("{0} is invalid time.", newRow.LocalDateTimeTo));
            }

            newRow.UtcDateTimeFrom = timeZone.ConvertTimeToUtc(newRow.LocalDateTimeFrom, timeZone);
            newRow.UtcDateTimeTo = timeZone.ConvertTimeToUtc(newRow.LocalDateTimeTo, timeZone);

            ForecastParseResult<int> integerResult;
            if (!_integerValidator.TryParse(content[3], out integerResult))
            {
                throw new ValidationException(integerResult.ErrorMessage);
            }
            newRow.Tasks = integerResult.Value;

            ForecastParseResult<double> doubleResult;
            if (!_doubleValidator.TryParse(content[4], out doubleResult))
            {
                throw new ValidationException(doubleResult.ErrorMessage);
            }
            newRow.TaskTime = doubleResult.Value;

            if (!_doubleValidator.TryParse(content[5], out doubleResult))
            {
                throw new ValidationException(doubleResult.ErrorMessage);
            }
            newRow.AfterTaskTime = doubleResult.Value;

            if (content.Length > ForecastsFileConstants.FileColumnsWithoutAgent)
            {
                if (!_doubleValidator.TryParse(content[6], out doubleResult))
                {
                    throw new ValidationException(doubleResult.ErrorMessage);
                }

                newRow.Agents = doubleResult.Value;
            }
            return newRow;
        }
    }
}