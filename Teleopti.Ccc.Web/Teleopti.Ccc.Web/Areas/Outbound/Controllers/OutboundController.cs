using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.Web.Filters;
using Task = System.Threading.Tasks.Task;

namespace Teleopti.Ccc.Web.Areas.Outbound.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class OutboundController : ApiController
	{
		
		[HttpPost, Route("api/Outbound/Campaign"), UnitOfWork]
		public virtual Task<CampaignViewModel> CreateCampaign(string name)
		{
			var campaignVm = CampaignViewModelFactory.Create(name);

			return Task.FromResult(campaignVm);
		}
	}
}