using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class UserTokenController : ApiController
	{
		private readonly IPersonalSettingDataRepository _settingsRepository;

		public UserTokenController(IPersonalSettingDataRepository settingsRepository)
		{
			_settingsRepository = settingsRepository;
		}

		[Route("start/usertoken"),HttpPost, UnitOfWork]
		public virtual IHttpActionResult Post([FromBody]string token)
		{
			var currentSetting = _settingsRepository.FindValueByKey(UserDevices.Key, new UserDevices());
			if (!currentSetting.TokenList.Contains(token))
			{
				currentSetting.AddToken(token);
				_settingsRepository.PersistSettingValue(UserDevices.Key, currentSetting);
			}
			return Ok();
		}

		[Route("start/usertoken"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult Get()
		{
			var currentSetting = _settingsRepository.FindValueByKey(UserDevices.Key, new UserDevices());
			return Ok(currentSetting.TokenList.ToArray());
		}
	}
}