using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class ImportJobArtifactValidator : IImportJobArtifactValidator
	{
		public virtual JobResultArtifact ValidateJobArtifact(IJobResult jobResult, Action<string> sendProgress )
		{
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
			return inputFile;
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

	public interface IImportJobArtifactValidator
	{
		JobResultArtifact ValidateJobArtifact(IJobResult jobResult, Action<string> sendProgress);
	}
}
