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
			_personalSettingDataRepository.PersistSettingValue(DataProtectionResponse.Key, 
				new DataProtectionResponse
				{
					Response = DataProtectionEnum.Yes,
					ResponseDate = DateTime.UtcNow
				});
			return Ok();
		}

		[UnitOfWork, Route("api/Global/DataProtection/No"), HttpPost]
		public virtual IHttpActionResult No()
		{
			_personalSettingDataRepository.PersistSettingValue(DataProtectionResponse.Key, 
				new DataProtectionResponse
				{
					Response = DataProtectionEnum.No,
					ResponseDate = DateTime.UtcNow
				});
			return Ok();
		}
	}
}