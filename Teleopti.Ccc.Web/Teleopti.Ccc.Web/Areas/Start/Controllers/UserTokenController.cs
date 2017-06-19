using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Notification;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class UserTokenController : ApiController
	{
		private readonly UserDeviceService _userDeviceService;

		public UserTokenController(UserDeviceService userDeviceService)
		{
			_userDeviceService = userDeviceService;
		}

		[Route("start/usertoken"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult Post([FromBody]string token)
		{
			_userDeviceService.StoreUserDevice(token);
			return Ok();
		}

		[Route("start/usertoken"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult Get()
		{
			var tokens = _userDeviceService.GetUserTokens();
			return Ok(tokens);
		}
	}
}