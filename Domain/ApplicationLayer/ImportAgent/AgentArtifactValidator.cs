using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentJobArtifactValidator : IImportAgentJobArtifactValidator
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IFileProcessor _fileProcess;

		public ImportAgentJobArtifactValidator(IJobResultRepository jobResultRepository, IFileProcessor fileProcess)
		{
			_jobResultRepository = jobResultRepository;
			_fileProcess = fileProcess;
		}
		[UnitOfWork]
		public virtual AgentFileProcessResult ValidateJobArtifact(ImportAgentEvent @event, Action<string> sendProgress )
		{
			var jobResult = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			if (jobResult == null)
			{
				sendProgress("Can not found the job.");
				return null;
			}
			var version = jobResult.Version;
			if (version.GetValueOrDefault() > 1)
			{
				sendProgress($"Job has been processed, version number: {version}");
				return null;
			}
			var currentVersion = jobResult.Version.GetValueOrDefault();
			jobResult.SetVersion(++currentVersion);

			sendProgress("Start to do input artifact validation!");
			JobResultArtifact inputFile;
			var errorMsg = validateJobInputArtifact(jobResult, out inputFile);
			sendProgress("Done input artifact validation!");
			if (!errorMsg.IsNullOrEmpty())
			{
				saveJobResultDetail(jobResult, errorMsg, DetailLevel.Error);
				sendProgress($"Input artifact validate has error:{errorMsg}");
				return null;
			}
			var processResult = _fileProcess.Process(new FileData
			{
				Data = inputFile.Content,
				FileName = inputFile.Name
			}, @event.Defaults, sendProgress);
			processResult.TimezoneForCreator = jobResult.Owner.PermissionInformation.DefaultTimeZone();
			processResult.InputArtifactInfo = new {inputFile.FileName, inputFile.FileType};
			return processResult;
		}

		private string validateJobInputArtifact(IJobResult jobResult, out JobResultArtifact inputFile)
		{
			inputFile = null;
			if (jobResult.Artifacts.IsNullOrEmpty())
			{
				return Resources.NoInput;
			}

			if (jobResult.Artifacts.Count > 1)
			{
				return Resources.InvalidInput;
			}

			inputFile = jobResult.Artifacts.FirstOrDefault(a => a.Category == JobResultArtifactCategory.Input);
			if ((inputFile?.Content?.Length ?? 0) == 0)
			{
				return Resources.InvalidInput;
			}
			return string.Empty;
		}

		private void saveJobResultDetail(IJobResult result, string message, DetailLevel level = DetailLevel.Info, Exception exception = null)
		{
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;
		}
	}

	public interface IImportAgentJobArtifactValidator
	{
		AgentFileProcessResult ValidateJobArtifact(ImportAgentEvent @event, Action<string> sendProgress);
	}
}
