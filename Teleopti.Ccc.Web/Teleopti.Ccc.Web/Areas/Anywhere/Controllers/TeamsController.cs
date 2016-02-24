using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class TeamsController : ApiController
	{
		private readonly IGetTeamAdherence _getTeamAdherence;
		private readonly IGetBusinessUnitId _getBusinessUnitId;

		public TeamsController(IGetTeamAdherence getTeamAdherence,
			IGetBusinessUnitId getBusinessUnitId)
		{
			_getTeamAdherence = getTeamAdherence;
			_getBusinessUnitId = getBusinessUnitId;
		}

		[UnitOfWork, HttpGet, Route("api/Teams/ForSite")]
		public virtual IHttpActionResult ForSite(Guid siteId)
		{
			return Ok(_getTeamAdherence.ForSite(siteId));
		}
		
		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Teams/GetOutOfAdherenceForTeamsOnSite")]
		public virtual IHttpActionResult GetOutOfAdherenceForTeamsOnSite(Guid siteId)
		{
			return Ok(_getTeamAdherence.GetOutOfAdherenceForTeamsOnSite(siteId));
		}

		[UnitOfWork, HttpGet, Route("api/Teams/GetBusinessUnitId")]
		public virtual IHttpActionResult GetBusinessUnitId(Guid teamId)
		{
			return Ok(_getBusinessUnitId.Get(teamId));
		}
	}
}