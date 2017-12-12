using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ExternalPerformanceInfoFileProcessor : IExternalPerformanceInfoFileProcessor
	{
		private const int customizedFieldCount = 8;
		private const int maxLengthOfAgentId = 130;
		private const int maxLengthOfGameName = 200;
		private const int maxCountOfMeasure = 10;
		private const string gameTypeNumberic = "numeric";
		private const string gameTypePercent = "percent";

		private const int dateColumnIndex = 0;
		private const int agentIdColumnIndex = 1;
		private const int gameNameColumnIndex = 4;
		private const int gameIdColumnIndex = 5;
		private const int gameTypeColumnIndex = 6;

		private readonly IExternalPerformanceRepository _externalPerformanceRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ITenantLogonDataManager _tenantLogonDataManager;

		public ExternalPerformanceInfoFileProcessor(IExternalPerformanceRepository externalPerformanceRepository,
			IPersonRepository personRepository, ITenantLogonDataManager tenantLogonDataManager)
		{
			_externalPerformanceRepository = externalPerformanceRepository;
			_personRepository = personRepository;
			_tenantLogonDataManager = tenantLogonDataManager;
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

		private ExternalPerformanceInfoProcessResult extractFileWithoutSeperateSession(byte[] rawData,
			Action<string> sendProgress)
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

			var extractResultCollection = new List<PerformanceInfoExtractionResult>();
			foreach (var currentLine in allLines)
			{
				var extractionResult = new PerformanceInfoExtractionResult
				{
					RawLine = currentLine
				};

				var columns = currentLine.Split(',').Select(x => x.TrimStart('"').TrimEnd('"')).ToArray();

				// Verify columns count
				if (columns.Length != customizedFieldCount)
				{
					var errorMessage = string.Format(Resources.InvalidNumberOfFields, customizedFieldCount, columns.Length);
					processResult.InvalidRecords.Add($"{currentLine},{errorMessage}");
					continue;
				}

				// Verify date
				if (!DateTime.TryParseExact(columns[dateColumnIndex], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,
					out var dateTime))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.ImportBpoWrongDateFormat}");
					continue;
				}
				extractionResult.DateFrom = new DateOnly(dateTime);

				// Verify game name
				var gameName = columns[gameNameColumnIndex];
				if (!verifyFieldLength(gameName, maxLengthOfGameName))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.GameNameIsTooLong}");
					continue;
				}
				extractionResult.GameName = gameName;

				// Verify game type
				var gameType = columns[gameTypeColumnIndex].ToLower();
				if (gameType != gameTypeNumberic && gameType != gameTypePercent)
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidGameType}");
					continue;
				}
				extractionResult.GameType = gameType;

				// Verify score based on game type
				if (extractionResult.GameType == gameTypeNumberic)
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

				// Verify agent id
				var agentId = columns[agentIdColumnIndex].Trim();
				if (!verifyFieldLength(agentId, maxLengthOfAgentId))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.AgentIdIsTooLong}");
					continue;
				}
				extractionResult.AgentId = agentId;

				// Verify game id
				if (!int.TryParse(columns[gameIdColumnIndex], out var gameId))
				{
					processResult.InvalidRecords.Add($"{currentLine},{Resources.InvalidGameId}");
					continue;
				}
				extractionResult.GameId = gameId;

				// If external performance not exists yet, try add a new one
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
					if (allMeasures.Count == maxCountOfMeasure)
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

				extractResultCollection.Add(extractionResult);
			}

			if (extractResultCollection.Any())
			{
				// Try find person with EmploymentNumber or ExternalLogon match with identity first
				var extractResultCollectionToBeMatchAppLogonName = new List<PerformanceInfoExtractionResult>();
				var personIdentityList = extractResultCollection.Select(x => x.AgentId).Distinct();
				var personIdentitiesByEmploymentNumberAndExternalLogon =
					_personRepository.FindPersonByIdentities(personIdentityList);
				foreach (var extractionResult in extractResultCollection)
				{
					var identify = extractionResult.AgentId;

					if (personIdentitiesByEmploymentNumberAndExternalLogon.All(x => x.Identity != identify))
					{
						extractResultCollectionToBeMatchAppLogonName.Add(extractionResult);
						continue;
					}

					addMatchPerformanceData(extractionResult, personIdentitiesByEmploymentNumberAndExternalLogon, processResult);
				}

				// If not found, then try find person with ApplicationLogonName match with identity
				if (extractResultCollectionToBeMatchAppLogonName.Any())
				{
					personIdentityList = extractResultCollectionToBeMatchAppLogonName.Select(x => x.AgentId).Distinct();
					var personIdentiesByAppLogonName = _tenantLogonDataManager.GetLogonInfoForIdentities(personIdentityList).ToList();
					foreach (var extractionResult in extractResultCollectionToBeMatchAppLogonName)
					{
						var identity = extractionResult.AgentId;
						if (personIdentiesByAppLogonName.All(x => x.Identity != identity))
						{
							processResult.InvalidRecords.Add($"{extractionResult.RawLine},{Resources.AgentDoNotExist}");
							continue;
						}
						addMatchPerformanceData(extractionResult, personIdentiesByAppLogonName.Select(x => new PersonIdentityMatchResult
						{
							Identity = x.Identity,
							PersonId = x.PersonId,
							MatchField = IdentityMatchField.ApplicationLogonName
						}).ToList(), processResult);
					}
				}
			}

			processResult.ValidRecords = processResult.ValidRecords.GroupBy(r => new {r.PersonId, r.DateFrom})
				.Select(x => x.Last()).ToList();
			processResult.ExternalPerformances =
				allMeasures.Where(m => existMeasures.All(e => e.ExternalId != m.ExternalId)).ToList();
			return processResult;
		}

		private static void addMatchPerformanceData(PerformanceInfoExtractionResult extractionResult,
			IList<PersonIdentityMatchResult> personIdentities,
			ExternalPerformanceInfoProcessResult processResult)
		{
			var matchPersonIdentities = personIdentities.Where(x =>
				x.Identity == extractionResult.AgentId && x.MatchField == IdentityMatchField.EmploymentNumber).ToList();
			if (!matchPersonIdentities.Any())
			{
				matchPersonIdentities = personIdentities.Where(x =>
					x.Identity == extractionResult.AgentId && x.MatchField == IdentityMatchField.ExternalLogon).ToList();
				if (!matchPersonIdentities.Any())
				{
					matchPersonIdentities = personIdentities.Where(x =>
						x.Identity == extractionResult.AgentId && x.MatchField == IdentityMatchField.ApplicationLogonName).ToList();
				}
			}

			if (!matchPersonIdentities.Any()) return;

			var agentsWithSameExternalLogon = matchPersonIdentities.Select(x => x.PersonId)
				.Select(personId => new PerformanceInfoExtractionResult
				{
					AgentId = extractionResult.AgentId,
					DateFrom = extractionResult.DateFrom,
					GameId = extractionResult.GameId,
					GameName = extractionResult.GameName,
					GameNumberScore = extractionResult.GameNumberScore,
					GamePercentScore = extractionResult.GamePercentScore,
					GameType = extractionResult.GameType,
					PersonId = personId
				});
			processResult.ValidRecords.AddRange(agentsWithSameExternalLogon);
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
			var fileName = importFileData.FileName.ToLower();
			return fileName.EndsWith("csv");
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
