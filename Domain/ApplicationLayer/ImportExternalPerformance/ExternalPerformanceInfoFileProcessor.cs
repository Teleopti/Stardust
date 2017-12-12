using System;
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
	public class ExternalPerformanceInfoFileProcessor : IExternalPerformanceInfoFileProcessor
	{
		private readonly int CUSTOMIZED_FIELD_COUNT = 8;
		private readonly int MAX_AGENT_ID_LENGTH = 130;
		private readonly int MAX_GAME_NAME_LENGTH = 200;
		private readonly int MAX_MEASURE_COUNT = 10;
		private readonly string GAME_TYPE_NUMBERIC = "numeric";
		private readonly string GAME_TYPE_PERCENT = "percent";

		private readonly IExternalPerformanceRepository _externalPerformanceRepository;
		private readonly IPersonRepository _personRepository;

		public ExternalPerformanceInfoFileProcessor(IExternalPerformanceRepository externalPerformanceRepository, IPersonRepository personRepository)
		{
			_externalPerformanceRepository = externalPerformanceRepository;
			_personRepository = personRepository;
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
			sendProgress("ExternalPerformanceInfoFileProcessor: Start to extract file.");
			var processResult = validateFileContent(rawData);

			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.ErrorMessages);
				sendProgress($"ExternalPerformanceInfoFileProcessor: Extract file has error:{msg}.");
				return processResult;
			}

			sendProgress("ExternalPerformanceInfoFileProcessor: Extract file succeed.");
			return processResult;
		}

		private ExternalPerformanceInfoProcessResult validateFileContent(byte[] content)
		{
			var processResult = new ExternalPerformanceInfoProcessResult();
			var allLines = byteArrayToString(content);
			var existMeasures = _externalPerformanceRepository.FindAllExternalPerformances().ToList();
			var allMeasures = new List<IExternalPerformance>();
			allMeasures.AddRange(existMeasures);

			foreach (var roughCurrentLine in allLines)
			{
				var currentLine = roughCurrentLine.Replace("\"", string.Empty);
				var extractionResult = new PerformanceInfoExtractionResult();
				var columns = currentLine.Split(',');
				if (columns.Length != CUSTOMIZED_FIELD_COUNT)
				{
					var errorMessage = string.Format(Resources.InvalidNumberOfFields, CUSTOMIZED_FIELD_COUNT, columns.Length);
					processResult.InvalidRecords.Add($"{currentLine},{errorMessage}");
					continue;
				}

				if (!DateTime.TryParseExact(columns[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.ImportBpoWrongDateFormat}");
					continue;
				}
				extractionResult.DateFrom = new DateOnly(dateTime);

				var measureName = columns[4];
				if (!verifyFieldLength(measureName, MAX_GAME_NAME_LENGTH))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.GameNameIsTooLong}");
					continue;
				}
				extractionResult.GameName = measureName;

				if (columns[6].ToLower() != GAME_TYPE_NUMBERIC && columns[6].ToLower() != GAME_TYPE_PERCENT)
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidGameType}");
					continue;
				}
				extractionResult.GameType = columns[6].ToLower();

				if (extractionResult.GameType == GAME_TYPE_NUMBERIC)
				{
					if (int.TryParse(columns.Last(), out var score)) extractionResult.GameNumberScore = score;
					else
					{
						processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidScore}");
						continue;
					}
				}
				else
				{
					if (Percent.TryParse(columns.Last(), out var score)) extractionResult.GamePercentScore = score;
					else
					{
						processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidScore}");
						continue;
					}
				}

				var agentId = columns[1];
				if (!verifyFieldLength(agentId, MAX_AGENT_ID_LENGTH))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.AgentIdIsTooLong}");
					continue;
				}
				extractionResult.AgentId = agentId;

				if (!int.TryParse(columns[5], out var gameId))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidGameId}");
					continue;
				}
				extractionResult.GameId = gameId;

				var measure = allMeasures.FirstOrDefault(g => g.ExternalId == extractionResult.GameId);
				if (measure != null)
				{
					if (convertMeasureType(measure.DataType) != extractionResult.GameType)
					{
						processResult.InvalidRecords.Add($"{currentLine},{Resources.GameTypeChanged}");
						continue;
					}
				}
				else
				{
					if (allMeasures.Count == MAX_MEASURE_COUNT)
					{
						processResult.InvalidRecords.Add($"{currentLine},{Resources.OutOfMaximumLimit}");
						continue;
					}
					allMeasures.Add(new ExternalPerformance
					{
						DataType = convertMeasureToDataType(extractionResult.GameType),
						ExternalId = extractionResult.GameId,
						Name = extractionResult.GameName
					});
				}

				var personIdentities = new Dictionary<string, IList<Guid>>();
				var identify = extractionResult.AgentId.Trim();
				var agentsWithSameExternalLogon = new List<PerformanceInfoExtractionResult>();

				IList<Guid> personIdList;
				if (personIdentities.ContainsKey(identify))
				{
					personIdList = personIdentities[identify];
				}
				else
				{
					// TODO-xinfli: Should change to query with batch identity
					personIdList = _personRepository.FindPersonByIdentity(identify);
					if (!personIdList.Any())
					{
						processResult.InvalidRecords.Add($"{currentLine},{Resources.AgentDoNotExist}");
						continue;
					}
					personIdentities.Add(identify, personIdList);
				}

				foreach (var personId in personIdList)
				{
					var eachExternalLogonInfo = new PerformanceInfoExtractionResult
					{
						AgentId = extractionResult.AgentId,
						DateFrom = extractionResult.DateFrom,
						GameId = extractionResult.GameId,
						GameName = extractionResult.GameName,
						GameNumberScore = extractionResult.GameNumberScore,
						GamePercentScore = extractionResult.GamePercentScore,
						GameType = extractionResult.GameType,
						PersonId = personId
					};
					agentsWithSameExternalLogon.Add(eachExternalLogonInfo);
				}

				processResult.ValidRecords.AddRange(agentsWithSameExternalLogon);
			}

			processResult.ValidRecords = processResult.ValidRecords.GroupBy(r => new {r.PersonId, r.DateFrom})
				.Select(x => x.Last()).ToList();
			processResult.ExternalPerformances =
				allMeasures.Where(m => existMeasures.All(e => e.ExternalId != m.ExternalId)).ToList();
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
