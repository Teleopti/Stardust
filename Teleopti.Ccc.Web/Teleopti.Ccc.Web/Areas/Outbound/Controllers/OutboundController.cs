using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.Web.Filters;
using Task = System.Threading.Tasks.Task;

namespace Teleopti.Ccc.Web.Areas.Outbound.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class OutboundController : ApiController
	{
		private readonly IOutboundCampaignPersister _outboundCampaignPersister;

		public OutboundController(IOutboundCampaignPersister outboundCampaignPersister)
		{
			_outboundCampaignPersister = outboundCampaignPersister;
		}

		[HttpPost, Route("api/Outbound/Campaign"), UnitOfWork]
		public virtual Task<CampaignViewModel> CreateCampaign(string name)
		{
			var campaignVm = _outboundCampaignPersister.Persist(name);

			return Task.FromResult(campaignVm);
		}
	}
}