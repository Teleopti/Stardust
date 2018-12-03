using System;
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

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ImportExternalPerformanceInfoHandler : IHandleEvent<ImportExternalPerformanceInfoEvent>, IRunOnStardust
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IStardustJobFeedback _feedback;
		private readonly IImportJobArtifactValidator _validator;
		private readonly IExternalPerformanceInfoFileProcessor _externalPerformanceInfoFileProcessor;
		private readonly IExternalPerformancePersister _persister;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportExternalPerformanceInfoHandler));

		public ImportExternalPerformanceInfoHandler(IJobResultRepository jobResultRepository, IStardustJobFeedback feedback, 
			IImportJobArtifactValidator validator, IExternalPerformanceInfoFileProcessor externalPerformanceInfoFileProcessor,
			IExternalPerformancePersister externalPerformancePersister)
		{
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_validator = validator;
			_externalPerformanceInfoFileProcessor = externalPerformanceInfoFileProcessor;
			_persister = externalPerformancePersister;
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
				SaveJobResultDetail(@event, e);
				throw;
			}
		}


		[UnitOfWork]
		protected virtual void SaveJobResultDetail(ImportExternalPerformanceInfoEvent @event, Exception e)
		{
			SaveJobResultDetailInternal(@event, DetailLevel.Error, e.Message, e);
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

			_feedback.SendProgress($"Starting to process {inputFile.Name}");

			var processResult = _externalPerformanceInfoFileProcessor.Process(new ImportFileData
			{
				Data = inputFile.Content,
				FileName = inputFile.Name
			});

			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.ErrorMessages);
				_feedback.SendProgress($"An error occurred while extracting: {msg}");
				SaveJobResultDetailInternal(@event, DetailLevel.Error, msg, null);
				return;
			}

			_feedback.SendProgress($"Extracted {processResult.ValidRecords.Count()} valid record(s), {processResult.InvalidRecords.Count()} invalid record(s). Starting to save data");

			SaveJobArtifactsAndUpdateRecords(@event, processResult, inputFile.Name);
		}

		private void SaveJobArtifactsAndUpdateRecords(ImportExternalPerformanceInfoEvent @event, ExternalPerformanceInfoProcessResult processResult, string fileName)
		{
			if (processResult.InvalidRecords.Any())
			{
				var jobResult = _jobResultRepository.FindWithNoLock(@event.JobResultId);
				jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.OutputError, "invalid_records_" + fileName, stringToArray(processResult.InvalidRecords)));
				_feedback.SendProgress($"Saved csv file of invalid records");
			}

			_persister.Persist(processResult);

			SaveJobResultDetailInternal(@event, DetailLevel.Info, "Updated External Performance Data", null);
		}

		private void SaveJobResultDetailInternal(ImportExternalPerformanceInfoEvent @event, DetailLevel level, string message, Exception ex)
		{
			var result = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, ex);
			result.AddDetail(detail);
			result.FinishedOk = true;

			_feedback.SendProgress($"Added Job Result Detail. Detail level: {level}, message: {message}, exception: {ex?.ToString() ?? "null"}");
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
