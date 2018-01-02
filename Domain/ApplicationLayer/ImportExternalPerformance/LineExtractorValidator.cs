using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class LineExtractorValidator : ILineExtractorValidator
	{
		private const int numColumn = 8;

		private const int personIdMaxLength = 130;
		private const int measureNameMaxLength = 200;

		private const int dateColumnIndex = 0;
		private const int personIdColumnIndex = 1;
		private const int measureNameColumnIndex = 4;
		private const int measureIdColumnIndex = 5;
		private const int measureTypeColumnIndex = 6;
		private const int measureScoreColumnIndex = 7;

		private const string dateFormat = "yyyyMMdd";

		public PerformanceInfoExtractionResult ExtractAndValidate(string line)
		{
			var result = new PerformanceInfoExtractionResult { RawLine = line };

			var columns = line.Split(',').Select(x => x.TrimStart('"').TrimEnd('"')).ToArray();

			if (!lineHasEnoughColumns(columns.Length, numColumn))
			{
				var errorMessage = string.Format(Resources.InvalidNumberOfFields, numColumn, columns.Length);
				result.Error = $"{line},{errorMessage}";
				return result;
			}

			if (!dateFieldIsValid(columns[dateColumnIndex], dateFormat, out var dateTime))
			{
				result.Error = $"{line},{Resources.ImportBpoWrongDateFormat}";
				return result;
			}
			result.DateFrom = new DateTime(dateTime.Ticks, DateTimeKind.Utc);

			var measureName = columns[measureNameColumnIndex];
			if (!measureNameLengthIsValid(measureName))
			{
				result.Error = $"{line},{Resources.GameNameIsTooLong}";
				return result;
			}
			result.MeasureName = measureName;

			var measureType = columns[measureTypeColumnIndex].ToLower();
			if (!measureTypeIsEitherNumericOrPercent(measureType, out var mtype))
			{
				result.Error = $"{line},{Resources.MeasureTypeMustBeEitherNumericOrPercent}";
				return result;
			}
			result.MeasureType = mtype;

			if (result.MeasureType == ExternalPerformanceDataType.Numeric)
			{
				if (measureTypeIsValidDecimalNumber(columns[measureScoreColumnIndex], out var score))
				{
					result.MeasureNumberScore = score;
				}
				else
				{
					result.Error = $"{line},{Resources.InvalidScore}";
					return result;
				}
			}
			else
			{
				if (measureTypeIsValidPercentage(columns[measureScoreColumnIndex], out var score))
				{
					result.MeasurePercentScore = score;
				}
				else
				{
					result.Error = $"{line},{Resources.InvalidScore}";
					return result;
				}
			}

			var personId = columns[personIdColumnIndex].Trim();
			if (!personIdLengthIsValid(personId))
			{
				result.Error = $"{line},{Resources.PersonIdIsTooLong}";
				return result;
			}
			result.AgentId = personId;

			if (!measureIdContainsInteger(columns[measureIdColumnIndex], out var gameId))
			{
				result.Error = $"{line},{Resources.MeasureIdMustContainAnInteger}";
				return result;
			}
			result.MeasureId = gameId;

			return result;
		}

		private bool verifyFieldLength(string field, int maxLength)
		{
			return field.Length <= maxLength;
		}

		private bool lineHasEnoughColumns(int line, int required)
		{
			return line == required;
		}

		private bool dateFieldIsValid(string value, string format, out DateTime result)
		{
			return DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
		}

		private bool measureNameLengthIsValid(string name)
		{
			return verifyFieldLength(name, measureNameMaxLength);
		}

		private bool measureTypeIsEitherNumericOrPercent(string value, out ExternalPerformanceDataType result)
		{
			return Enum.TryParse(value, true, out result);
		}

		private bool personIdLengthIsValid(string id)
		{
			return verifyFieldLength(id, personIdMaxLength);
		}

		private bool measureIdContainsInteger(string id, out int result)
		{
			return int.TryParse(id, out result);
		}

		private bool measureTypeIsValidDecimalNumber(string value, out int result)
		{
			return int.TryParse(value, out result);
		}

		private bool measureTypeIsValidPercentage(string value, out Percent result)
		{
			return Percent.TryParse(value, out result);
		}
	}
}
