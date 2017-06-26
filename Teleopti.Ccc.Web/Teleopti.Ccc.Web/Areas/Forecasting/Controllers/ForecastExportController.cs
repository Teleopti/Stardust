using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting.Export.Web;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class ForecastExportController : ApiController
	{
		private readonly ForecastExportModelCreator _forecastExportModelCreator;

		public ForecastExportController(ForecastExportModelCreator forecastExportModelCreator)
		{
			_forecastExportModelCreator = forecastExportModelCreator;
		}

		[HttpPost, Route("api/Forecasting/Export"), UnitOfWork]
		public virtual HttpResponseMessage Export(ExportForecastInput input)
		{
			var response = new HttpResponseMessage();
			var exportModel = _forecastExportModelCreator.Load(input.WorkloadId, new DateOnlyPeriod(new DateOnly(input.ForecastStart.Date), new DateOnly(input.ForecastEnd.Date)));

			var excelExport = new ForecastExportToExcel();
			
			response.Content = new ByteArrayContent(excelExport.Export(exportModel));
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			return response;
		}
	}
}