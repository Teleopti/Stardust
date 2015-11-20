using System.Web.Http;
using Teleopti.Ccc.Web.Areas.Start.Core.Config;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class ConfigController : ApiController
	{
		private readonly ISharedSettingsFactory _sharedSettingsFactory;

		public ConfigController(ISharedSettingsFactory sharedSettingsFactory)
		{
			_sharedSettingsFactory = sharedSettingsFactory;
		}

		[HttpGet,Route("Start/Config/SharedSettings")]
		public virtual IHttpActionResult SharedSettings()
		{
			return Ok(_sharedSettingsFactory.Create());
		}
	}
}