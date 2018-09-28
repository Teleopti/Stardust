﻿using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.ExportSchedule)]
	public class ExportScheduleController : ApiController
	{
		private readonly ExportScheduleService _exportScheduleService;

		public ExportScheduleController(ExportScheduleService exportScheduleService)
		{
			_exportScheduleService = exportScheduleService;
		}


		[HttpPost, UnitOfWork, Route("api/TeamSchedule/StartExport")]
		public virtual HttpResponseMessage StartExport([FromBody]ExportScheduleForm input)
		{
			var result = _exportScheduleService.ExportToExcel(input);

			return createResponse(result, "TeamsExportedSchedules.xlsx");
		}

		private HttpResponseMessage createResponse(ProcessExportResult result, string fileName)
		{
			var response = new HttpResponseMessage();
			if (!result.FailReason.IsNullOrEmpty())
			{
				response.Headers.Add("Message", result.FailReason);
				return response;
			}
			response.Content = new ByteArrayContent(result.Data);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			response.Content.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
			return response;
		}
	}
}