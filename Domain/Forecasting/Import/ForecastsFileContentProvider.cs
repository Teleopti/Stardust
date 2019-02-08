using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
	public interface IForecastsFileContentProvider
	{
		ICollection<ForecastsRow> LoadContent(byte[] fileContent, TimeZoneInfo timeZone);
	}

	public class ForecastsFileContentProvider : IForecastsFileContentProvider
	{
		private readonly IForecastsRowExtractor _rowExtractor;

		public ForecastsFileContentProvider(IForecastsRowExtractor rowExtractor)
		{
			_rowExtractor = rowExtractor;
		}

		public ICollection<ForecastsRow> LoadContent(byte[] fileContent, TimeZoneInfo timeZone)
		{
			var rowNumber = 1;
			var result = new List<ForecastsRow>();
			try
			{
				var fileContentString = Encoding.UTF8.GetString(fileContent).TrimStart('\uFEFF');
				var fileRows = fileContentString.Split(new[] {Environment.NewLine},
					StringSplitOptions.RemoveEmptyEntries).ToList();

				var firstRow = fileRows.FirstOrDefault();
				if (firstRow == null) throw new ValidationException("Imported forecast file is empty.");

				_rowExtractor.PresetTokenSeparator(firstRow);
				
				if (_rowExtractor.IsValidHeaderRow(fileRows[0]))
					fileRows = fileRows.Skip(1).ToList();
				
				foreach (var line in fileRows)
				{
					var forecastsRow = _rowExtractor.Extract(line, timeZone);
					if (timeZone.IsAmbiguousTime(forecastsRow.LocalDateTimeFrom))
					{
						var missingRow = new ForecastsRow
						{
							AfterTaskTime = forecastsRow.AfterTaskTime,
							Agents = forecastsRow.Agents,
							LocalDateTimeFrom = forecastsRow.LocalDateTimeFrom,
							LocalDateTimeTo = forecastsRow.LocalDateTimeTo,
							SkillName = forecastsRow.SkillName,
							Tasks = forecastsRow.Tasks,
							TaskTime = forecastsRow.TaskTime,
							UtcDateTimeFrom = adjustMissingUtcTime(forecastsRow.UtcDateTimeFrom, timeZone),
							UtcDateTimeTo = adjustMissingUtcTime(forecastsRow.UtcDateTimeTo, timeZone)
						};
						result.Add(missingRow);
					}
					else
					{
						if (timeZone.IsAmbiguousTime(forecastsRow.LocalDateTimeTo))
							forecastsRow.UtcDateTimeTo = adjustMissingUtcTime(forecastsRow.UtcDateTimeTo,
								timeZone);
					}
					result.Add(forecastsRow);
					rowNumber++;
				}
			}
			catch (ValidationException exception)
			{
				throw new ValidationException(string.Format(CultureInfo.InvariantCulture, "Line {0}, Error:{1}", rowNumber, exception.Message));
			}
			return result;
		}

		private static DateTime adjustMissingUtcTime(DateTime utcTime, TimeZoneInfo timeZone)
		{
			var rules = timeZone.GetAdjustmentRules();
			var rul = rules.FirstOrDefault(r => r.DateStart < new DateTime(utcTime.Year, 1, 1) && r.DateEnd > new DateTime(utcTime.Year, 1, 1));
			return rul == null ? utcTime : utcTime.Add(-rul.DaylightDelta);
		}
	}
}