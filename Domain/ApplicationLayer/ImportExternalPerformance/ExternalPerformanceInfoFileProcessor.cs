﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ExternalPerformanceInfoFileProcessor : IExternalPerformanceInfoFileProcessor
	{
		private const int MAX_MEASURE_COUNT = 10;

		private readonly IExternalPerformanceRepository _externalPerformanceRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ILineExtractor _lineExtractor;
		private readonly IExternalPerformanceDataRepository _externalDataRepository;
		private readonly ITenantLogonPersonProvider _tenantLogonPersonProvider;

		public ExternalPerformanceInfoFileProcessor(IExternalPerformanceRepository externalPerformanceRepository,
			IPersonRepository personRepository, 
			ILineExtractor lineExtractor,
			IExternalPerformanceDataRepository externalDataRepository, ITenantLogonPersonProvider tenantLogonPersonProvider)
		{
			_externalPerformanceRepository = externalPerformanceRepository;
			_personRepository = personRepository;
			_lineExtractor = lineExtractor;
			_externalDataRepository = externalDataRepository;
			_tenantLogonPersonProvider = tenantLogonPersonProvider;
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
			var processResult = ExtractFileContent(rawData);

			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.ErrorMessages);
				sendProgress($"ExternalPerformanceInfoFileProcessor: Extract file has error:{msg}.");
				return processResult;
			}

			sendProgress("ExternalPerformanceInfoFileProcessor: Extract file succeed.");
			return processResult;
		}

		private ExternalPerformanceInfoProcessResult ExtractFileContent(byte[] content)
		{
			var processResult = new ExternalPerformanceInfoProcessResult();
			var allLines = byteArrayToString(content);

			var existMeasures = _externalPerformanceRepository.FindAllExternalPerformances().ToList();
			var allMeasures = new List<IExternalPerformance>();
			allMeasures.AddRange(existMeasures);

			var extractResultCollection = new List<PerformanceInfoExtractionResult>();
			foreach (var line in allLines)
			{
				var extractionResult = _lineExtractor.ExtractAndValidate(line);

				if (extractionResult.HasError())
				{
					processResult.InvalidRecords.Add(extractionResult.Error);
					continue;
				}

				CreateOrValidatePerformanceType(extractionResult, allMeasures);

				if (extractionResult.HasError())
				{
					processResult.InvalidRecords.Add(extractionResult.Error);
					continue;
				}

				extractResultCollection.Add(extractionResult);
			}

			if (extractResultCollection.Any())
			{
				// Try find person with EmploymentNumber or ExternalLogon match with identity first
				ValidateAgentId(processResult, extractResultCollection);
			}

			processResult.ValidRecords = processResult.ValidRecords.GroupBy(r => new {r.PersonId, r.DateFrom}).Select(x => x.Last()).ToList();
			processResult.ExternalPerformances = allMeasures.Where(m => existMeasures.All(e => e.ExternalId != m.ExternalId)).ToList();
			return processResult;
		}

		private void ValidateAgentId(ExternalPerformanceInfoProcessResult processResult, List<PerformanceInfoExtractionResult> extractResultCollection)
		{
			var extractResultCollectionToBeMatchAppLogonName = new List<PerformanceInfoExtractionResult>();
			var logonNames = extractResultCollection.Select(x => x.AgentId).Distinct();
			var personIdentitiesByEmploymentNumberAndExternalLogon = _personRepository.FindPersonByIdentities(logonNames);
			foreach (var extractionResult in extractResultCollection)
			{
				if (personIdentitiesByEmploymentNumberAndExternalLogon.All(x => x.LogonName != extractionResult.AgentId))
				{
					extractResultCollectionToBeMatchAppLogonName.Add(extractionResult);
					continue;
				}

				addMatchPerformanceData(extractionResult, personIdentitiesByEmploymentNumberAndExternalLogon, processResult);
			}

			//If not found, then try find person with ApplicationLogonName match with identity
			if (extractResultCollectionToBeMatchAppLogonName.Any())
			{
				logonNames = extractResultCollectionToBeMatchAppLogonName.Select(x => x.AgentId).Distinct();
				var personInfoModels = _tenantLogonPersonProvider.GetByLogonNames(logonNames).ToList();
				foreach (var extractionResult in extractResultCollectionToBeMatchAppLogonName)
				{
					if (personInfoModels.All(x => x.ApplicationLogonName != extractionResult.AgentId))
					{
						processResult.InvalidRecords.Add($"{extractionResult.RawLine},{Resources.AgentDoNotExist}");
						continue;
					}
					addMatchPerformanceData(extractionResult, personInfoModels.Select(x => new PersonIdentityMatchResult
					{
						LogonName = x.ApplicationLogonName,
						PersonId = x.PersonId,
						MatchField = IdentityMatchField.ApplicationLogonName
					}).ToList(), processResult);
				}
			}
		}

		private void CreateOrValidatePerformanceType(PerformanceInfoExtractionResult extractionResult, IList<IExternalPerformance> existingTypes)
		{
			var type = existingTypes.FirstOrDefault(g => g.ExternalId == extractionResult.GameId);
			if (type != null)
			{
				if (type.DataType != extractionResult.GameType)
				{
					extractionResult.Error = $"{extractionResult.RawLine},{Resources.GameTypeChanged}";
					return;
				}
			}
			else
			{
				if (existingTypes.Count == MAX_MEASURE_COUNT)
				{
					extractionResult.Error = $"{extractionResult.RawLine},{Resources.OutOfMaximumLimit}";
					return;
				}
				existingTypes.Add(new ExternalPerformance
				{
					DataType = extractionResult.GameType,
					ExternalId = extractionResult.GameId,
					Name = extractionResult.GameName
				});
			}
		}

		private static void addMatchPerformanceData(PerformanceInfoExtractionResult extractionResult,
			IList<PersonIdentityMatchResult> personIdentities,
			ExternalPerformanceInfoProcessResult processResult)
		{
			var matchPersonIdentities = personIdentities.Where(x =>
				x.LogonName == extractionResult.AgentId && x.MatchField == IdentityMatchField.EmploymentNumber).ToList();
			if (!matchPersonIdentities.Any())
			{
				matchPersonIdentities = personIdentities.Where(x =>
					x.LogonName == extractionResult.AgentId && x.MatchField == IdentityMatchField.ExternalLogon).ToList();
				if (!matchPersonIdentities.Any())
				{
					matchPersonIdentities = personIdentities.Where(x =>
						x.LogonName == extractionResult.AgentId && x.MatchField == IdentityMatchField.ApplicationLogonName).ToList();
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
