using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class LineExtractor : ILineExtractor
	{
		private const int NUM_COLUMN = 8;

		private const int AGENT_ID_MAX_LENGTH = 130;
		private const int GAME_NAME_MAX_LENGTH = 200;

		private const int DATE_COLUMN_INDEX = 0;
		private const int AGENT_ID_COLUMN_INDEX = 1;
		private const int GAME_NAME_COLUMN_INDEX = 4;
		private const int GAME_ID_COLUMN_INDEX = 5;
		private const int GAME_TYPE_COLUMN_INDEX = 6;
		private const int GAME_SCORE_COLUMN_INDEX = 7;

		private const string DATE_FORMAT = "yyyyMMdd";

		public PerformanceInfoExtractionResult ExtractAndValidate(string line)
		{
			var result = new PerformanceInfoExtractionResult { RawLine = line };

			var columns = line.Split(',').Select(x => x.TrimStart('"').TrimEnd('"')).ToArray();

			if (!LineHasEnoughColumns(columns.Length, NUM_COLUMN))
			{
				var errorMessage = string.Format(Resources.InvalidNumberOfFields, NUM_COLUMN, columns.Length);
				result.Error = $"{line},{errorMessage}";
				return result;
			}

			if (!DateFieldIsValid(columns[DATE_COLUMN_INDEX], DATE_FORMAT, out var dateTime))
			{
				result.Error = $"{line},{Resources.ImportBpoWrongDateFormat}";
				return result;
			}
			result.DateFrom = new DateTime(dateTime.Ticks, DateTimeKind.Utc);

			var gameName = columns[GAME_NAME_COLUMN_INDEX];
			if (!GameNameFieldIsValid(gameName))
			{
				result.Error = $"{line},{Resources.GameNameIsTooLong}";
				return result;
			}
			result.GameName = gameName;

			var gameType = columns[GAME_TYPE_COLUMN_INDEX].ToLower();
			if (!GameTypeFieldIsValid(gameType, out var gtype))
			{
				result.Error = $"{line},{Resources.InvalidGameType}";
				return result;
			}
			result.GameType = gtype;

			if (result.GameType == ExternalPerformanceDataType.Numeric)
			{
				if (GameTypeIsValidNumber(columns[GAME_SCORE_COLUMN_INDEX], out var score))
				{
					result.GameNumberScore = score;
				}
				else
				{
					result.Error = $"{line},{Resources.InvalidScore}";
					return result;
				}
			}
			else
			{
				if (GameTypeIsValidPercentage(columns[GAME_SCORE_COLUMN_INDEX], out var score))
				{
					result.GamePercentScore = score;
				}
				else
				{
					result.Error = $"{line},{Resources.InvalidScore}";
					return result;
				}
			}

			var agentId = columns[AGENT_ID_COLUMN_INDEX].Trim();
			if (!AgentIdFieldIsValid(agentId))
			{
				result.Error = $"{line},{Resources.AgentIdIsTooLong}";
				return result;
			}
			result.AgentId = agentId;

			if (!GameIdFieldIsValid(columns[GAME_ID_COLUMN_INDEX], out var gameId))
			{
				result.Error = $"{line},{Resources.InvalidGameId}";
				return result;
			}
			result.GameId = gameId;

			return result;
		}

		private bool VerifyFieldLength(string field, int maxLength)
		{
			return field.Length <= maxLength;
		}

		private bool LineHasEnoughColumns(int line, int required)
		{
			return line == required;
		}

		private bool DateFieldIsValid(string value, string format, out DateTime result)
		{
			return DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
		}

		private bool GameNameFieldIsValid(string name)
		{
			return VerifyFieldLength(name, GAME_NAME_MAX_LENGTH);
		}

		private bool GameTypeFieldIsValid(string value, out ExternalPerformanceDataType result)
		{
			return Enum.TryParse(value, true, out result);
		}

		private bool AgentIdFieldIsValid(string id)
		{
			return VerifyFieldLength(id, AGENT_ID_MAX_LENGTH);
		}

		private bool GameIdFieldIsValid(string id, out int result)
		{
			return int.TryParse(id, out result);
		}

		private bool GameTypeIsValidNumber(string value, out int result)
		{
			return int.TryParse(value, out result);
		}

		private bool GameTypeIsValidPercentage(string value, out Percent result)
		{
			return Percent.TryParse(value, out result);
		}
	}
}
