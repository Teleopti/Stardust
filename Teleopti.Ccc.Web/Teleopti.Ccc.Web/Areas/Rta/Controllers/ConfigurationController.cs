using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Configuration;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class ConfigurationController : ApiController
	{
		private readonly IConfigurationValidator _validator;

		public ConfigurationController(IConfigurationValidator validator)
		{
			_validator = validator;
		}

		[HttpGet, Route("Rta/Configuration/Validate")]
		[TenantScope]
		[UnitOfWork]
		public virtual IHttpActionResult Validate([FromUri] string tenant = null)
		{
			return Ok(_validator.Validate());
		}
	}
}