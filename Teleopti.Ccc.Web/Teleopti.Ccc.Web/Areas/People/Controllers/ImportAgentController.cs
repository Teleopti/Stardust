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
			return _importAgentJobService.GetJobsForLoggedOnBusinessUnit()?.Select(detail => new
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
				ErrorMessage = detail.HasError ? detail.ResultDetail?.Message ?? Resources.InternalErrorMessage : string.Empty
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
		[Route("UploadAgent"), HttpPost]
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

		[UnitOfWork]
		protected virtual HttpResponseMessage ProcessInternal(IEnumerable<HttpContent> contents)
		{
			var formData = _multipartHttpContentExtractor.ExtractFormModel<ImportAgentDefaults>(contents);
			var fileData = _multipartHttpContentExtractor.ExtractFileData(contents);
			var workbook = _fileProcessor.ParseFile(fileData.SingleOrDefault());
			var isXlsx = workbook is XSSFWorkbook;

			var errorMsg = _fileProcessor.ValidateSheetColumnHeader(workbook);
			if (!errorMsg.IsNullOrEmpty())
			{
				var invalidFileResponse = Request.CreateResponse((HttpStatusCode)422);
				invalidFileResponse.Headers.Clear();
				invalidFileResponse.Headers.Add("Message", $"format errors: {errorMsg}");

				invalidFileResponse.Content = new StringContent(errorMsg);
				invalidFileResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				return invalidFileResponse;
			}
			var total = _fileProcessor.GetNumberOfRecordsInSheet(workbook.GetSheetAt(0));
			var invalidAgents = _fileProcessor.ProcessSheet(workbook.GetSheetAt(0), _userTimeZone.TimeZone(), formData).ToList();

			var successCount = total - invalidAgents.Count;
			var failedCount = invalidAgents.Count(a => a.Feedback.ErrorMessages.Any());
			var warningCount = invalidAgents.Count(a => (!a.Feedback.ErrorMessages.Any()) && a.Feedback.WarningMessages.Any());

			var response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.Clear();
			response.Headers.Add("Message", $"success count:{successCount}, failed count:{failedCount}, warning count:{warningCount}");

			if (!invalidAgents.Any())
			{
				return response;
			}

			var ms = _fileProcessor.CreateFileForInvalidAgents(invalidAgents.ToList(), isXlsx);
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