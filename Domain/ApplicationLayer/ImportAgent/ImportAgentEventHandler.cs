using System;
using System.Linq;
using NPOI.HSSF.UserModel;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentEventHandler : IHandleEvent<ImportAgentEvent>,
		IRunOnStardust
	{
		private IJobResultRepository _jobResultRepository;
		private IFileProcessor _fileProcessor;

		public ImportAgentEventHandler(
			IJobResultRepository jobResultRepository,
			IFileProcessor fileProcessor,
			INow now
		 )
		{
			_jobResultRepository = jobResultRepository;
			_fileProcessor = fileProcessor;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(ImportAgentEvent @event)
		{
			var jobResult = _jobResultRepository.Get(@event.JobResultId);

			if (jobResult.Artifacts.IsNullOrEmpty())
			{
				SaveJobResultDetail(jobResult, Resources.NoInput, DetailLevel.Error);
				return;
			}
			if (jobResult.Artifacts.Count > 1)
			{
				SaveJobResultDetail(jobResult, Resources.InvalidInput, DetailLevel.Error);
				return;
			}

			var inputFile = jobResult.Artifacts.FirstOrDefault(a => a.Category == JobResultArtifactCategory.Input);
			if (inputFile == null || inputFile.Content == null || inputFile.Content.Length == 0)
			{
				SaveJobResultDetail(jobResult, Resources.InvalidInput, DetailLevel.Error);
			}
			else
			{
				var workbook = _fileProcessor.ParseFile(new FileData
				{
					Data = inputFile.Content,
					FileName = inputFile.Name
				});

				var error = _fileProcessor.ValidateWorkbook(workbook);
				if (!error.IsNullOrEmpty())
				{
					SaveJobResultDetail(jobResult, error, DetailLevel.Error);
					return;
				}

				var result = _fileProcessor.Process(workbook.GetSheetAt(0), @event.Defaults);
				var total = _fileProcessor.GetNumberOfRecordsInSheet(workbook.GetSheetAt(0));
				var faildAgents = result.ExtractedResults.Where(a => a.Feedback.ErrorMessages.Any());
				var warningAgents = result.ExtractedResults.Where(a => (!a.Feedback.ErrorMessages.Any()) && a.Feedback.WarningMessages.Any());

				var failedCount = faildAgents.Count();
				var warningCount = warningAgents.Count();
				var successCount = total - failedCount - warningCount;

				var fileNameLength = inputFile.Name.Length;
				var fileTypeIndex = inputFile.Name.LastIndexOf(".");
				string fileName = inputFile.Name.Substring(0, fileNameLength - fileTypeIndex);
				string fileType = inputFile.Name.Substring(fileTypeIndex + 1, fileNameLength - fileTypeIndex - 1);
				var level = DetailLevel.Info;
				if (failedCount > 0)
				{
					var faildFile = _fileProcessor.CreateFileForInvalidAgents(faildAgents.ToList(),
						fileType.Equals("xlsx", StringComparison.OrdinalIgnoreCase));

					jobResult.AddArtifact(new JobResultArtifact(
						JobResultArtifactCategory.OutputError,
						$"{fileName}_error.{fileType}",
						faildFile.ToArray()));

					level = DetailLevel.Error;
				}
				if (warningCount > 0)
				{
					var warningFile = _fileProcessor.CreateFileForInvalidAgents(warningAgents.ToList(),
						fileType.Equals("xlsx", StringComparison.OrdinalIgnoreCase));

					jobResult.AddArtifact(new JobResultArtifact(
						JobResultArtifactCategory.OutputWarning,
						$"{fileName}_warning.{fileType}",
						warningFile.ToArray()));
				}

				if (warningCount > 0 && failedCount == 0)
				{
					level = DetailLevel.Warning;
				}

				SaveJobResultDetail(jobResult, $"success count:{successCount}, failed count:{failedCount}, warning count:{warningCount}", level);
			}
		}



		private void SaveJobResultDetail(IJobResult result, string message, DetailLevel level = DetailLevel.Info, Exception exception = null)
		{
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;
		}

	}
}
