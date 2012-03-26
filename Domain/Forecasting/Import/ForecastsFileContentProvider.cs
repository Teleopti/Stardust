using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastsFileContentProvider
    {
        ICollection<IForecastsFileRow> LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone, TimeSpan midnightBreakOffset);
    }

    public class ForecastsFileContentProvider : IForecastsFileContentProvider
    {
        private readonly IForecastsRowExtractor _rowExtractor;

        public ForecastsFileContentProvider(IForecastsRowExtractor rowExtractor)
        {
            _rowExtractor = rowExtractor;
        }

        public ICollection<IForecastsFileRow> LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone, TimeSpan midnightBreakOffset)
        {
            return Encoding.UTF8.GetString(fileContent).Split(new[] {Environment.NewLine},
                                                           StringSplitOptions.RemoveEmptyEntries).Select(
                                                               r => _rowExtractor.Extract(r, timeZone, midnightBreakOffset)).ToList();
        }
    }

    public class ForecastsRowExtractor : IForecastsRowExtractor
    {
        private readonly ISpecification<string[]> _columnsInRowValidSpecification = new IsColumnCountInRowValid();
        private readonly ForecastsFileSkillNameValidator _skillNameValidator = new ForecastsFileSkillNameValidator();
        private readonly ForecastsFileDateTimeValidator _dateTimeValidator = new ForecastsFileDateTimeValidator();
        private readonly ForecastsFileIntegerValueValidator _integerValidator = new ForecastsFileIntegerValueValidator();
        private readonly ForecastsFileDoubleValueValidator _doubleValidator = new ForecastsFileDoubleValueValidator();

        public IForecastsFileRow Extract(string rowString, ICccTimeZoneInfo timeZone, TimeSpan midnightBreakOffset)
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

            newRow.UtcDateTimeFrom = timeZone.ConvertTimeToUtc(newRow.LocalDateTimeFrom.Add(midnightBreakOffset), timeZone);
            newRow.UtcDateTimeTo = timeZone.ConvertTimeToUtc(newRow.LocalDateTimeTo.Add(midnightBreakOffset), timeZone);

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

            if (content.Length > 6)
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

    public class ForecastParseResult<T>
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public T Value { get; set; }
    }

    public interface IForecastsRowExtractor
    {
        IForecastsFileRow Extract(string rowString, ICccTimeZoneInfo timeZone, TimeSpan midnightBreakOffset);
    }

    public class IsColumnCountInRowValid : Specification<string[]>
    {
        public override bool IsSatisfiedBy(string[] obj)
        {
            return obj.Length == ForecastsFileConstants.FileColumnsWithoutAgent || obj.Length == ForecastsFileConstants.FileColumnsWithAgent;
        }
    }
}