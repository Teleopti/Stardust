﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface IExternalPerformanceInfoFileProcessor
	{
		ExternalPerformanceInfoProcessResult Process(ImportFileData importFileData, Action<string> sendProgress);
	}

	public class PerformanceInfoExtractionResult
	{
		public DateTime DateFrom { get; set; }
		public string GameType { get; set; }
		public string GameName { get; set; }
		public int GameId { get; set; }
		public string AgentId { get; set; }
		public int GameNumberScore { get; set; }
		public Percent GamePercentScore { get; set; }
	}

	public class ExternalPerformanceInfoProcessResult
	{
		public ExternalPerformanceInfoProcessResult()
		{
			ValidRecords = new List<PerformanceInfoExtractionResult>();
			InvalidRecords = new List<string>();
			ExternalPerformances = new List<IExternalPerformance>();
		}
		public bool HasError { get; set; }
		public string ErrorMessages { get; set; }
			
		public int TotalRecordCount => InvalidRecords.Count + ValidRecords.Count;
		public IList<string> InvalidRecords { get; set; }

		public List<PerformanceInfoExtractionResult> ValidRecords { get; set; }
		public List<IExternalPerformance> ExternalPerformances { get; set; }
	}

	public class ExternalPerformanceInfoFileProcessor : IExternalPerformanceInfoFileProcessor
	{
		private readonly int CUSTOMIZED_FIELD_COUNT = 8;
		private readonly int MAX_AGENT_ID_LENGTH = 100;
		private readonly int MAX_GAME_NAME_LENGTH = 200;
		private readonly int MAX_MEASURE_COUNT = 10;
		private readonly string GAME_TYPE_NUMBERIC = "numeric";
		private readonly string GAME_TYPE_PERCENT = "percent";

		private readonly IExternalPerformanceRepository _externalPerformanceRepository;

		public ExternalPerformanceInfoFileProcessor(IExternalPerformanceRepository externalPerformanceRepository)
		{
			_externalPerformanceRepository = externalPerformanceRepository;
		}

		public ExternalPerformanceInfoProcessResult Process(ImportFileData importFileData, Action<string> sendProgress)
		{
			var processResult = new ExternalPerformanceInfoProcessResult();

			if (!isFileTypeValid(importFileData))
			{
				processResult.HasError = true;
				processResult.ErrorMessages = Resources.InvalidInput;

				var msg = string.Join(", ", processResult.ErrorMessages);
				sendProgress($"ExternalPerformanceInfoFileProcessor: Extract file has error:{msg}.");
				return processResult;
			}

			var fileProcessResult = extractFileWithoutSeperateSession(importFileData.Data, sendProgress);
			return fileProcessResult;
		}
		
		private ExternalPerformanceInfoProcessResult extractFileWithoutSeperateSession(byte[] rawData, Action<string> sendProgress)
		{
			sendProgress($"ExternalPerformanceInfoFileProcessor: Start to extract file.");
			var processResult = validateFileContent(rawData);

			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.ErrorMessages);
				sendProgress($"ExternalPerformanceInfoFileProcessor: Extract file has error:{msg}.");
				return processResult;
			}

			sendProgress($"ExternalPerformanceInfoFileProcessor: Extract file succeed.");
			return processResult;
		}

		private ExternalPerformanceInfoProcessResult validateFileContent(byte[] content)
		{
			var processResult = new ExternalPerformanceInfoProcessResult();
			var allLines = byteArrayToString(content);
			var existMeasures = _externalPerformanceRepository.FindAllExternalPerformances();
			var allMeasures = new List<IExternalPerformance>();
			allMeasures.AddRange(existMeasures);

			foreach (var currentLine in allLines)
			{
				var extractionResult = new PerformanceInfoExtractionResult();
				var columns = currentLine.Split(',');
				if (columns.Length != CUSTOMIZED_FIELD_COUNT)
				{
					processResult.HasError = true;
					var errorMessage = string.Format(Resources.InvalidNumberOfFields, CUSTOMIZED_FIELD_COUNT, columns.Length);
					processResult.InvalidRecords.Add($"{currentLine},{errorMessage}");
					continue;
				}

				DateTime dateTime;
				if (!DateTime.TryParseExact(columns[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
				{
					processResult.HasError = true;
					processResult.InvalidRecords.Add($"{currentLine},{Resources.ImportBpoWrongDateFormat}");
					continue;
				}
				extractionResult.DateFrom = dateTime;

				var measureName = columns[4];
				if (!verifyFieldLength(measureName, MAX_GAME_NAME_LENGTH))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.GameNameIsTooLong}");
					continue;
				}
				extractionResult.GameName = measureName;

				if (columns[6].ToLower() != GAME_TYPE_NUMBERIC && columns[6].ToLower() != GAME_TYPE_PERCENT)
				{
					processResult.HasError = true;
					processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidGameType}");
					continue;
				}
				extractionResult.GameType = columns[6].ToLower();

				if (extractionResult.GameType == GAME_TYPE_NUMBERIC)
				{
					int score;
					if (int.TryParse(columns.Last(), out score)) extractionResult.GameNumberScore = score;
					else
					{
						processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidScore}");
						continue;
					}
				}
				else
				{
					Percent score;
					if (Percent.TryParse(columns.Last(), out score)) extractionResult.GamePercentScore = score;
					else
					{
						processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidScore}");
						continue;
					}
				}

				var agentId = columns[1];
				if (!verifyFieldLength(agentId, MAX_AGENT_ID_LENGTH))
				{
					processResult.HasError = true;
					processResult.InvalidRecords.Add($"{currentLine},{Resources.AgentIdIsTooLong}");
					continue;
				}
				extractionResult.AgentId = agentId;

				int gameId;
				if (!int.TryParse(columns[5], out gameId))
				{
					processResult.HasError = true;
					processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidGameId}");
					continue;
				}
				extractionResult.GameId = gameId;

				if (allMeasures.Count == MAX_MEASURE_COUNT)
				{
					processResult.HasError = true;
					processResult.InvalidRecords.Add($"{currentLine},{Resources.OutOfMaximumLimit}");
					continue;
				}
				var measure = allMeasures.FirstOrDefault(g => g.ExternalId == extractionResult.GameId);
				if (measure != null)
				{
					if (convertMeasureType(measure.DataType) != extractionResult.GameType)
					{
						processResult.HasError = true;
						processResult.InvalidRecords.Add($"{currentLine},{Resources.GameTypeChanged}");
						continue;
					}
				}
				else
				{
					allMeasures.Add(new ExternalPerformance()
					{
						DataType = convertMeasureToDataType(extractionResult.GameType),
						ExternalId = extractionResult.GameId,
						Name = extractionResult.GameName
					});
				}
				
				//check agent


				processResult.ValidRecords.Add(extractionResult);
			}

			return processResult;
		}

		private ExternalPerformanceDataType convertMeasureToDataType(string type)
		{
			switch (type)
			{
				case "number":
					return ExternalPerformanceDataType.Numeric;
				case "percent":
					return ExternalPerformanceDataType.Percentage;
				default:
					return ExternalPerformanceDataType.Numeric;
			}
		}
		private string convertMeasureType(ExternalPerformanceDataType type)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return "number";
				case ExternalPerformanceDataType.Percentage:
					return "percent";
				default:
					return "";
			}
		}

		private bool verifyFieldLength(string field, int maxLength)
		{
			return field.Length <= maxLength;
		}

		private bool isFileTypeValid(ImportFileData importFileData)
		{
			var fileName = importFileData.FileName;
			if (!fileName.ToLower().EndsWith("csv"))
				return false;
			
			return true;
		}

		private IList<string> byteArrayToString(byte[] byteArray)
		{
			var records = new List<string>();
			using (var ms = new MemoryStream(byteArray))
			{
				using (var sr = new StreamReader(ms))
				{

					while (!sr.EndOfStream)
					{
						var record = sr.ReadLine();
						records.Add(record);
					}
				}
			}
			return records;
		}
	}
}
