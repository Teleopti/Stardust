using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Mart.Controllers
{
	[KeyBasedAuthentication] // Enable authentication via a key
	[System.Web.Http.Authorize] // Require some form of authentication
	public class QueueStatsController : ApiController
	{
		private readonly IQueueStatHandler _queueStatHandler;

		public QueueStatsController(IQueueStatHandler queueStatHandler)
		{
			_queueStatHandler = queueStatHandler;
		}

		
		public void Post([FromBody]QueueStatsModel value)
		{
			// would be possible to send the datasource (the name in the nhib) as a claim
			var principal =  RequestContext.Principal as ClaimsPrincipal;
			Claim claim = null;
			string dataSource = "";
			if (principal != null)
				claim = principal.Claims.FirstOrDefault(c => c.Type.Equals(System.IdentityModel.Claims.ClaimTypes.Locality));
			if (claim != null)
				dataSource = claim.Value;

			if (!string.IsNullOrEmpty(dataSource))
				_queueStatHandler.Handle(value, dataSource);
		}
	}
}