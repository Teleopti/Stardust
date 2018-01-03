using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ExternalPerformanceInfoFileProcessor : IExternalPerformanceInfoFileProcessor
	{
		private const int maxMeasureCount = 10;

		private readonly IExternalPerformanceRepository _externalPerformanceRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ILineExtractorValidator _lineExtractorValidator;
		private readonly ITenantLogonPersonProvider _tenantLogonPersonProvider;

		public ExternalPerformanceInfoFileProcessor(IExternalPerformanceRepository externalPerformanceRepository,
			IPersonRepository personRepository, 
			ILineExtractorValidator lineExtractorValidator,
			ITenantLogonPersonProvider tenantLogonPersonProvider)
		{
			_externalPerformanceRepository = externalPerformanceRepository;
			_personRepository = personRepository;
			_lineExtractorValidator = lineExtractorValidator;
			_tenantLogonPersonProvider = tenantLogonPersonProvider;
		}

		public ExternalPerformanceInfoProcessResult Process(ImportFileData importFileData)
		{
			var processResult = new ExternalPerformanceInfoProcessResult();

			if (!isFileTypeValid(importFileData))
			{
				processResult.HasError = true;
				processResult.ErrorMessages = Resources.InvalidInput;
				return processResult;
			}

			var fileProcessResult = extractFileWithoutSeperateSession(importFileData.Data);
			return fileProcessResult;
		}

		private ExternalPerformanceInfoProcessResult extractFileWithoutSeperateSession(byte[] rawData)
		{
			var processResult = extractFileContent(rawData);
			return processResult;
		}

		private ExternalPerformanceInfoProcessResult extractFileContent(byte[] content)
		{
			var processResult = new ExternalPerformanceInfoProcessResult();
			var allLines = byteArrayToString(content);

			var existMeasures = _externalPerformanceRepository.FindAllExternalPerformances().ToArray();
			var allMeasures = existMeasures.ToList();

			var extractResultCollection = new List<PerformanceInfoExtractionResult>();
			foreach (var line in allLines)
			{
				var extractionResult = _lineExtractorValidator.ExtractAndValidate(line);

				if (extractionResult.HasError())
				{
					processResult.InvalidRecords.Add(extractionResult.Error);
					continue;
				}

				createOrValidatePerformanceType(extractionResult, allMeasures);

				if (extractionResult.HasError())
				{
					processResult.InvalidRecords.Add(extractionResult.Error);
					continue;
				}

				extractResultCollection.Add(extractionResult);
			}

			if (extractResultCollection.Any())
			{
				convertToRecordsByMatchingPersonId(extractResultCollection, processResult);
			}

			processResult.ValidRecords = processResult.ValidRecords.GroupBy(r => new {r.PersonId, r.DateFrom}).Select(x => x.Last()).ToList();
			processResult.ExternalPerformances = allMeasures.Where(m => existMeasures.All(e => e.ExternalId != m.ExternalId)).ToList();
			return processResult;
		}

		private void convertToRecordsByMatchingPersonId(IList<PerformanceInfoExtractionResult> allExtractionResults,
			ExternalPerformanceInfoProcessResult processResult)
		{
			var allPersonIds = allExtractionResults.Select(x => x.AgentId).ToArray();
			var allEmploymentNumberAndExternalLogonMatches = _personRepository.FindPersonByIdentities(allPersonIds).ToLookup(x => x.LogonName);
			var allApplicationLogonNameMatches = new Lazy<IPersonInfoModel[]>(() => _tenantLogonPersonProvider.GetByLogonNames(allPersonIds).ToArray());
			foreach (var extractionResult in allExtractionResults)
			{
				var employmentNumberAndExternalLogonMatches = allEmploymentNumberAndExternalLogonMatches[extractionResult.AgentId].ToLookup(t => t.MatchField, a => a.PersonId);
				var personIds = employmentNumberAndExternalLogonMatches[IdentityMatchField.EmploymentNumber].ToList();

				if (!personIds.Any())
				{
					personIds.AddRange(employmentNumberAndExternalLogonMatches[IdentityMatchField.ExternalLogon]);
				}

				if (!personIds.Any())
				{
					if (allApplicationLogonNameMatches.Value.Any())
					{
						var applicationLogonNameMatches = allApplicationLogonNameMatches.Value.Where(x => x?.ApplicationLogonName == extractionResult.AgentId);
						personIds.AddRange(applicationLogonNameMatches.Select(x => x.PersonId));
					}
				}

				if (!personIds.Any())
				{
					processResult.InvalidRecords.Add($"{extractionResult.RawLine},{Resources.PersonIdCouldNotBeMatchedToAnyAgent}");
					continue;
				}

				var validRecords = personIds.Select(personId => new PerformanceInfoExtractionResult
				{
					AgentId = extractionResult.AgentId,
					DateFrom = extractionResult.DateFrom,
					MeasureId = extractionResult.MeasureId,
					MeasureName = extractionResult.MeasureName,
					MeasureNumberScore = extractionResult.MeasureNumberScore,
					MeasurePercentScore = extractionResult.MeasurePercentScore,
					MeasureType = extractionResult.MeasureType,
					PersonId = personId
				});

				processResult.ValidRecords.AddRange(validRecords);
			}
		}

		private void createOrValidatePerformanceType(PerformanceInfoExtractionResult extractionResult, IList<IExternalPerformance> existingTypes)
		{
			var type = existingTypes.FirstOrDefault(g => g.ExternalId == extractionResult.MeasureId);
			if (type != null)
			{
				if (type.DataType != extractionResult.MeasureType)
				{
					extractionResult.Error = $"{extractionResult.RawLine},{Resources.MeasureTypeNotMatchExistingDefinition}";
				}
			}
			else
			{
				if (existingTypes.Count == maxMeasureCount)
				{
					extractionResult.Error =
						$"{extractionResult.RawLine},{string.Format(Resources.RowExceedsLimitOfGamificationMeasures, maxMeasureCount)}";
					return;
				}
				existingTypes.Add(new ExternalPerformance
				{
					DataType = extractionResult.MeasureType,
					ExternalId = extractionResult.MeasureId,
					Name = extractionResult.MeasureName
				});
			}
		}

		private bool isFileTypeValid(ImportFileData importFileData)
		{
			return importFileData.FileName.EndsWith("csv",StringComparison.InvariantCultureIgnoreCase);
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
