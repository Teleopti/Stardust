using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;

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

	[Serializable]
	public class UserDevices : SettingValue
	{
		public const string Key = "DevicesUserToken";

		private readonly ICollection<string> tokens = new HashSet<string>();

		public void AddToken(string token)
		{
			tokens.Add(token);
		}

		public IEnumerable<string> TokenList => tokens;
	}
}