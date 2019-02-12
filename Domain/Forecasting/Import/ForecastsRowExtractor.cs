using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastsRowExtractor
    {
        ForecastsRow Extract(string value, TimeZoneInfo timeZone);
		bool IsValidHeaderRow(string content);
		void PresetTokenSeparator(string templateRow);
		string HeaderRow { get; }
	}

    public class ForecastsRowExtractor : IForecastsRowExtractor
    {
        private readonly ISpecification<string[]> _columnsInRowValidSpecification = new IsColumnCountInRowValid();
        private readonly ForecastsFileSkillNameValidator _skillNameValidator = new ForecastsFileSkillNameValidator();
        private readonly ForecastsFileDateTimeUnifiedValidator _dateTimeValidator = new ForecastsFileDateTimeUnifiedValidator();
        private readonly ForecastsFileDoubleValueValidator _doubleValidator = new ForecastsFileDoubleValueValidator();
		private char _tokenSeparator = ',';
		
		public void PresetTokenSeparator(string templateRow)
		{
			_tokenSeparator = templateRow.Contains(";") ? ';' : ',';
		}
		
        public ForecastsRow Extract(string value, TimeZoneInfo timeZone)
        {
            var content = value.Split(_tokenSeparator);
            if (!_columnsInRowValidSpecification.IsSatisfiedBy(content))
            {
                throw new ValidationException($"There are more or less columns than expected when using '{_tokenSeparator}' as column separator.");
            }
            var newRow = new ForecastsRow();

			if (!_skillNameValidator.TryParse(content[0], out var stringResult))
            {
                throw new ValidationException(stringResult.ErrorMessage);
            }
            newRow.SkillName = stringResult.Value;

			if (!_dateTimeValidator.TryParse(content[1], out var dateTimeResult))
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

			if (!_doubleValidator.TryParse(content[3], out var tasksResult))
            {
                throw new ValidationException(tasksResult.ErrorMessage);
            }
            newRow.Tasks = tasksResult.Value;

			if (!_doubleValidator.TryParse(content[4], out var doubleResult))
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
			return content.Equals(HeaderRow) || content.Equals(HeaderRow.Replace(",",";"));
		}

		public string HeaderRow => "skillname,startdatetime,enddatetime,tasks,tasktime,aftertasktime,agents";
	}
}