using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules)]
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

			return createResponse(result.Data, "TeamsExportedSchedules.xlsx");
		}

		private HttpResponseMessage createResponse(byte[] data, string fileName)
		{
			var response = new HttpResponseMessage();
			response.Content = new ByteArrayContent(data);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			response.Content.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
			return response;
		}
	}
}