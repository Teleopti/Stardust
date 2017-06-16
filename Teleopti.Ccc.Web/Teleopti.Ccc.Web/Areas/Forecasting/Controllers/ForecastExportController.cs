using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting.Export.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class ForecastExportController : ApiController
	{
		[HttpPost, Route("api/Forecasting/Export"), UnitOfWork]
		public virtual IHttpActionResult Export(ExportForecastInput input)
		{
			return Ok();
		}
	}
}