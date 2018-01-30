using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public class ForecastsRowExtractorOld : IForecastsRowExtractor
    {
        private readonly ISpecification<string[]> _columnsInRowValidSpecification = new IsColumnCountInRowValid();
        private readonly ForecastsFileSkillNameValidator _skillNameValidator = new ForecastsFileSkillNameValidator();
        private readonly ForecastsFileDateTimeValidator _dateTimeValidator = new ForecastsFileDateTimeValidator();
        private readonly ForecastsFileIntegerValueValidator _integerValidator = new ForecastsFileIntegerValueValidator();
        private readonly ForecastsFileDoubleValueValidator _doubleValidator = new ForecastsFileDoubleValueValidator();
		
        public IForecastsRow Extract(string value, TimeZoneInfo timeZone)
        {
            var content = value.Split(',');
            if (!_columnsInRowValidSpecification.IsSatisfiedBy(content))
            {
                throw new ValidationException("There are more or less columns than expected.");
            }
            var newRow = new ForecastsRow();

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
                throw new ValidationException(string.Format(CultureInfo.InvariantCulture, "{0} is invalid time.", newRow.LocalDateTimeFrom));
            }
            
            if (!_dateTimeValidator.TryParse(content[2], out dateTimeResult))
            {
                throw new ValidationException(dateTimeResult.ErrorMessage);
            }
            newRow.LocalDateTimeTo = dateTimeResult.Value;
            if (timeZone.IsInvalidTime(newRow.LocalDateTimeTo))
            {
                throw new ValidationException(string.Format(CultureInfo.InvariantCulture, "{0} is invalid time.", newRow.LocalDateTimeTo));
            }

            newRow.UtcDateTimeFrom = timeZone.SafeConvertTimeToUtc(newRow.LocalDateTimeFrom);
            newRow.UtcDateTimeTo = timeZone.SafeConvertTimeToUtc(newRow.LocalDateTimeTo);

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
		
		public bool IsValidHeaderRow(string content)
		{
			return false;
		}

		public void PresetTokenSeparator(string templateRow)
		{
		}

		public string HeaderRow => "";
    }
}