using System;
using System.Linq;
using NPOI.HSSF.UserModel;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentEventHandler : IHandleEvent<ImportAgentEvent>,
		IRunOnStardust
	{
		private IJobResultRepository _jobResultRepository;
		private IFileProcessor _fileProcessor;

		public ImportAgentEventHandler(IJobResultRepository jobResultRepository, IFileProcessor fileProcessor)
		{
			_jobResultRepository = jobResultRepository;
			_fileProcessor = fileProcessor;
		}

		public void Handle(ImportAgentEvent @event)
		{
			var jobResult = _jobResultRepository.Get(@event.JobResultId);
			if (jobResult.Artifacts.IsNullOrEmpty())
			{
				jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, Resources.NoInput, DateTime.UtcNow, null));

			}
			else if (jobResult.Artifacts.Count > 1)
			{
				jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, Resources.InvalidInput, DateTime.UtcNow, null));
				
			}
			else
			{
				var inputFile = jobResult.Artifacts.FirstOrDefault(a => a.Category == JobResultArtifactCategory.Input);
				if (inputFile == null || inputFile.Content == null || inputFile.Content.Length == 0)
				{
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, Resources.InvalidInput, DateTime.UtcNow, null));
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
						jobResult.AddDetail((new JobResultDetail(DetailLevel.Error, error, DateTime.UtcNow, null)));
					}

					_fileProcessor.ProcessSheet(workbook.GetSheetAt(0), @event.Defaults);

				}
			}
			jobResult.FinishedOk = true;


		}
	}
}
