﻿using System.Collections.Generic;
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

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class ImportAgentController: ApiController
	{
		private readonly IImportAgentDataProvider _importAgentDataProvider;
		private readonly IFileProcessor _fileProcessor;
		private readonly IMultipartHttpContentExtractor _multipartHttpContentExtractor;

		private const string newExcelFileContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
		private const string oldExcelFileContentType = "application/vnd.ms-excel";

		public ImportAgentController(IImportAgentDataProvider importAgentDataProvider, IFileProcessor fileProcessor, IMultipartHttpContentExtractor multipartHttpContentExtractor)
		{
			_importAgentDataProvider = importAgentDataProvider;
			_fileProcessor = fileProcessor;
			_multipartHttpContentExtractor = multipartHttpContentExtractor;
		}

		[UnitOfWork, Route("api/People/GetImportAgentSettingsData"), HttpGet]
		public virtual ImportAgentsFieldOptionsModel GetImportAgentSettingsData()
		{
			return _importAgentDataProvider.FieldOptions();
		}

		[Route("api/People/UploadAgent"), HttpPost]
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
			var invalidAgents = _fileProcessor.ProcessSheet(workbook.GetSheetAt(0), formData).ToList();

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

		[Route("api/People/AgentTemplate"), HttpPost]
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
	}
}