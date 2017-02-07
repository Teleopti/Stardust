using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class DataProtectionController : ApiController
	{
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;

		public DataProtectionController(IPersonalSettingDataRepository personalSettingDataRepository)
		{
			_personalSettingDataRepository = personalSettingDataRepository;
		}

		[UnitOfWork, Route("api/Global/DataProtection/Yes"), HttpPost]
		public virtual IHttpActionResult Yes()
		{
			var setting = _personalSettingDataRepository.FindValueByKey(DataProtectionResponse.Key, new DataProtectionResponse());
			setting.Response = DataProtectionEnum.Yes;
			setting.ResponseDate = DateTime.UtcNow;
			_personalSettingDataRepository.PersistSettingValue(DataProtectionResponse.Key, setting);
			return Ok();
		}

		[UnitOfWork, Route("api/Global/DataProtection/No"), HttpPost]
		public virtual IHttpActionResult No()
		{
			var setting = _personalSettingDataRepository.FindValueByKey(DataProtectionResponse.Key, new DataProtectionResponse());
			setting.Response = DataProtectionEnum.No;
			setting.ResponseDate = DateTime.UtcNow;
			_personalSettingDataRepository.PersistSettingValue(DataProtectionResponse.Key, setting);
			return Ok();
		}
	}
}