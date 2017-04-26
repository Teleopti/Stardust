using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Castle.Core.Internal;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Web.Areas.People.Core;
using System.Web.Http.Results;
using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	[RoutePrefix("api/People")]
	public class ImportAgentController : ApiController
	{
		private readonly IImportAgentDataProvider _importAgentDataProvider;
		private readonly IFileProcessor _fileProcessor;
		private readonly IMultipartHttpContentExtractor _multipartHttpContentExtractor;

		private const string newExcelFileContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
		private const string oldExcelFileContentType = "application/vnd.ms-excel";
		private readonly IImportAgentJobService _importAgentJobService;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IUserTimeZone _userTimeZone;

		public ImportAgentController(IImportAgentDataProvider importAgentDataProvider,
			IFileProcessor fileProcessor,
			IMultipartHttpContentExtractor multipartHttpContentExtractor,
			IImportAgentJobService importAgentJobService,
			IPersonNameProvider personNameProvider,
			IUserTimeZone userTimeZone
			)
		{
			_importAgentDataProvider = importAgentDataProvider;
			_fileProcessor = fileProcessor;
			_multipartHttpContentExtractor = multipartHttpContentExtractor;
			_importAgentJobService = importAgentJobService;
			_personNameProvider = personNameProvider;
			_userTimeZone = userTimeZone;
		}

		[UnitOfWork, Route("GetImportAgentSettingsData"), HttpGet]
		public virtual ImportAgentsFieldOptionsModel GetImportAgentSettingsData()
		{
			return _importAgentDataProvider.FieldOptions();
		}

		[Route("NewImportAgentJob"), HttpPost]
		public async Task<OkResult> NewImportAgentJob()
		{
			var contents = await readAsMultipartAsync();
			var defaults = _multipartHttpContentExtractor.ExtractFormModel<ImportAgentDefaults>(contents);
			var fileData = _multipartHttpContentExtractor.ExtractFileData(contents).SingleOrDefault();
			CreateJob(fileData, defaults);
			return Ok();
		}

		[Route("AgentJobList"), HttpGet, UnitOfWork]
		public virtual object GetAgentJobList()
		{
			return _importAgentJobService.GetJobsForCurrentBusinessUnit()?.Select(detail => new
			{
				JobResultId = detail.JobResult.Id,
				Owner = _personNameProvider.BuildNameFromSetting(detail.JobResult.Owner.Name),
				detail.JobResult.Timestamp,
				detail.SuccessCount,
				detail.WarningCount,
				detail.FailedCount,
				IsWorking = detail.JobResult.IsWorking(),
				FailedArtifact = detail.FailedArtifact == null ? null : new
				{
					detail.FailedArtifact.Name,
					detail.FailedArtifact.Id
				},
				WarningArtifact = detail.WarningArtifact == null ? null : new
				{
					detail.WarningArtifact.Name,
					detail.WarningArtifact.Id
				},
				InputArtifact = detail.InputArtifact == null ? null : new
				{
					detail.InputArtifact.Name,
					detail.InputArtifact.Id
				},
				detail.HasError,
				ErrorMessage = detail.HasError ?
				(!detail.HasException ? detail.ResultDetail?.Message : Resources.InternalErrorMsg)
				: string.Empty
			})
			.ToList();
		}

		[Route("job/{id}/artifact/{category}"), HttpGet, UnitOfWork]
		public virtual HttpResponseMessage DownloadArtifact(Guid id, JobResultArtifactCategory category)
		{
			var response = Request.CreateResponse();
			var artifact = _importAgentJobService.GetJobResultArtifact(id, category);
			if (artifact == null)
			{
				response.StatusCode = HttpStatusCode.NotFound;
				return response;
			}

			response.Content = new ByteArrayContent(artifact.Content);
			var contentType = artifact.FileType.Equals("xlsx", StringComparison.OrdinalIgnoreCase)
				? newExcelFileContentType
				: oldExcelFileContentType;
			response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = artifact.Name
			};
			return response;
		}

		[UnitOfWork]
		public virtual IJobResult CreateJob(FileData fileData, ImportAgentDefaults defaults)
		{
			if (fileData?.Data?.Length == 0)
			{
				throw new ArgumentNullException(Resources.File, Resources.NoInput);
			}
			return _importAgentJobService.CreateJob(fileData, defaults);
		}

		[Route("UploadAgent"), HttpPost, RemoveMeWithToggle(Toggles.Wfm_People_ImportAndCreateAgentFromFile_42528)]
		public async Task<HttpResponseMessage> UploadAgent()
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
			}

			var provider = new MultipartMemoryStreamProvider();
			await Request.Content.ReadAsMultipartAsync(provider);

			return ProcessInternal(provider.Contents);
		}

		[RemoveMeWithToggle(Toggles.Wfm_People_ImportAndCreateAgentFromFile_42528)]
		private HttpResponseMessage ProcessInternal(IEnumerable<HttpContent> contents)
		{
			var formData = _multipartHttpContentExtractor.ExtractFormModel<ImportAgentDefaults>(contents);
			var fileData = _multipartHttpContentExtractor.ExtractFileData(contents).Single();
			var isXlsx = fileData.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
			var processResult = _fileProcessor.Process(fileData, _userTimeZone.TimeZone(), formData);

			if (processResult.Feedback.ErrorMessages.Any())
			{
				var errorMsg = string.Join(", ", processResult.Feedback.ErrorMessages);
				var invalidFileResponse = Request.CreateResponse((HttpStatusCode)422);
				invalidFileResponse.Headers.Clear();
				invalidFileResponse.Headers.Add("Message", $"format errors: {errorMsg}");
				invalidFileResponse.Content = new StringContent(errorMsg);
				return invalidFileResponse;
			}

			var invalidAgents = processResult.FailedAgents;
			invalidAgents.AddRange(processResult.WarningAgents);
			var response = Request.CreateResponse(HttpStatusCode.OK);
			var message = processResult.GetSummaryMessage();
			response.Headers.Clear();
			response.Headers.Add("Message", message);
			if (!invalidAgents.Any())
			{
				response.Content = new StringContent(message);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				return response;
			}

			var ms = _fileProcessor.CreateFileForInvalidAgents(invalidAgents, fileData.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase));
			response.Content = new ByteArrayContent(ms.ToArray());
			response.Content.Headers.ContentType =
				new MediaTypeHeaderValue(isXlsx ? newExcelFileContentType : oldExcelFileContentType);

			return response;
		}

		[Route("AgentTemplate"), HttpPost]
		public HttpResponseMessage GetFileTemplateAgent()
		{
			var template = new AgentFileTemplate();
			var ms = template.GetFileTemplate(template.GetDefaultAgent());
			var response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.Clear();
			response.Content = new ByteArrayContent(ms.ToArray());
			response.Content.Headers.ContentType = new MediaTypeHeaderValue(oldExcelFileContentType);

			return response;
		}


		private async Task<IEnumerable<HttpContent>> readAsMultipartAsync()
		{
			try
			{
				var provider = new MultipartMemoryStreamProvider();
				await Request.Content.ReadAsMultipartAsync(provider);
				return provider.Contents;
			}
			catch (ArgumentNullException e)
			{
				throw new ArgumentNullException(e.ParamName, Resources.NoInput);
			}

		}
	}


}