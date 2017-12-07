﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ImportExternalPerformanceInfoHandler : IHandleEvent<ImportExternalPerformanceInfoEvent>, IRunOnStardust
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IStardustJobFeedback _feedback;
		private readonly IImportJobArtifactValidator _validator;
		private readonly IExternalPerformanceInfoFileProcessor _externalPerformanceInfoFileProcessor;
		private readonly IExternalPerformanceRepository _externalPerformanceRepository;
		private readonly IExternalPerformanceDataReadModelRepository _dataReadModelRepository;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportExternalPerformanceInfoHandler));

		public ImportExternalPerformanceInfoHandler(IJobResultRepository jobResultRepository, IStardustJobFeedback feedback, 
			IImportJobArtifactValidator validator, IExternalPerformanceInfoFileProcessor externalPerformanceInfoFileProcessor, 
			IExternalPerformanceRepository externalPerformanceRepository, IExternalPerformanceDataReadModelRepository dataReadModelRepository)
		{
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_validator = validator;
			_externalPerformanceInfoFileProcessor = externalPerformanceInfoFileProcessor;
			_externalPerformanceRepository = externalPerformanceRepository;
			_dataReadModelRepository = dataReadModelRepository;
		}

		[AsSystem]
		[TenantScope]
		public virtual void Handle(ImportExternalPerformanceInfoEvent @event)
		{
			try
			{
				HandleJob(@event);
			}
			catch (Exception e)
			{
				Logger.Error(e);
				saveErrorJobResultDetail(@event, DetailLevel.Error, e.Message, e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void HandleJob(ImportExternalPerformanceInfoEvent @event)
		{
			var jobResult = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			var inputFile = _validator.ValidateJobArtifact(jobResult, _feedback.SendProgress);

			if (inputFile == null)
			{
				_feedback.SendProgress("ImportExternalPerformanceInfoHandler: Job artifact validation has error, give up!");
				return;
			}

			var processResult = _externalPerformanceInfoFileProcessor.Process(
				new ImportFileData
				{
					Data = inputFile.Content,
					FileName = inputFile.Name
				}, 
				_feedback.SendProgress);

			_feedback.SendProgress($"Done agent persist {processResult.TotalRecordCount}.");

			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.ErrorMessages);
				_feedback.SendProgress($"Extract file has error:{msg}.");
				saveErrorJobResultDetail(@event, DetailLevel.Error, msg, null);
				return;
			}

			saveJobArtifactsAndUpdateRecords(@event, processResult);
		}

		private void saveJobArtifactsAndUpdateRecords(ImportExternalPerformanceInfoEvent @event, ExternalPerformanceInfoProcessResult processResult)
		{
			var jobResult = _jobResultRepository.FindWithNoLock(@event.JobResultId);

			_feedback.SendProgress($"ImportExternalPerformanceInfoHandler Start to save job artifact!");
			if (processResult.InvalidRecords.Any())
			{
				jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "InvalidRecords.csv", stringToArray(processResult.InvalidRecords)));
				_jobResultRepository.Add(jobResult);
			}
			_feedback.SendProgress($"ImportExternalPerformanceInfoHandler Save job artifact success!");

			saveSettings(processResult.ExternalPerformances);
			saveRecords(processResult.ValidRecords);

			saveErrorJobResultDetail(@event, DetailLevel.Info, "", null);
		}

		private void saveSettings(IList<IExternalPerformance> externalPerformances )
		{
			foreach (var externalPerformance in externalPerformances)
			{
				_externalPerformanceRepository.Add(externalPerformance);
			}
		}

		private void saveRecords(IList<PerformanceInfoExtractionResult> validRecords)
		{
			var externalPerformanceSettings = _externalPerformanceRepository.FindAllExternalPerformances();

			foreach (var validRecord in validRecords)
			{
				var score = validRecord.GameNumberScore;
				if (validRecord.GameType == "percent") score = Convert.ToInt32(validRecord.GamePercentScore.Value * 10000);

				_dataReadModelRepository.Add(new ExternalPerformanceData()
					{
						ExternalPerformance = externalPerformanceSettings.FirstOrDefault(x => x.ExternalId == validRecord.GameId).Id.Value,
						DateFrom = validRecord.DateFrom,
						OriginalPersonId = validRecord.AgentId,
						Person = validRecord.PersonId,
						Score = score
					});
				}
		}

		private void saveErrorJobResultDetail(ImportExternalPerformanceInfoEvent @event, DetailLevel level, string message, Exception exception)
		{
			var result = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;

			_feedback.SendProgress($"ImportExternalPerformanceInfoHandler Done with adding job process detail, detail level:{level}, message:{message}, exception: {exception}.");
		}

		private byte[] stringToArray(IList<string> lines)
		{
			byte[] data;

			using (MemoryStream ms = new MemoryStream())
			{
				using (StreamWriter sw = new StreamWriter(ms))
				{
					foreach (var line in lines)
					{
						sw.WriteLine(line);

					}
					sw.Close();
				}
				data = ms.ToArray();
			}

			return data;
		}
	}
}
