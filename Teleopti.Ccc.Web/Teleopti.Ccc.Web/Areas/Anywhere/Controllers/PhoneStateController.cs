using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PhoneStateController : ApiController
	{
		private readonly PhoneStateViewModelBuilder _build;

		public PhoneStateController(PhoneStateViewModelBuilder build)
		{
			_build = build;
		}

		[UnitOfWork, HttpGet, Route("api/PhoneState/InfoFor")]
		public virtual IHttpActionResult InfoFor([FromUri] Query query)
		{
			return Ok(_build.For(query.Ids));
		}
	}
}