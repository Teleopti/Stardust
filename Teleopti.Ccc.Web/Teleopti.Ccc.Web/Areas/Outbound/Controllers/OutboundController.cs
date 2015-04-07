using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Outbound.Controllers
{
	public struct CampaignForm
	{
		public string Name { get; set; }
	}

	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class OutboundController : ApiController
	{
		private const string GivenDescriptionIsInvalidErrorMessage =
			"The given campaign name is invalid. It can contain at most 255 characters.";

		private readonly IOutboundCampaignPersister _outboundCampaignPersister;

		public OutboundController(IOutboundCampaignPersister outboundCampaignPersister)
		{
			_outboundCampaignPersister = outboundCampaignPersister;
		}

		[HttpPost, Route("api/Outbound/Campaign"), UnitOfWork]
		public virtual IHttpActionResult CreateCampaign([FromBody]CampaignForm campaignForm)
		{
			if (NameValidator.DescriptionIsInvalid(campaignForm.Name)) return BadRequest(GivenDescriptionIsInvalidErrorMessage);
			var campaignVm = _outboundCampaignPersister.Persist(campaignForm.Name);

			return Created(Request.RequestUri + "/" + campaignVm.Id, campaignVm);
		}
	}
}