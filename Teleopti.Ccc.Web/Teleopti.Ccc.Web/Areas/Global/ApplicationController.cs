using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ApplicationController : ApiController
	{
		private readonly IAreaWithPermissionPathProvider _areaWithPermissionPathProvider;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public ApplicationController(IAreaWithPermissionPathProvider areaWithPermissionPathProvider, IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_areaWithPermissionPathProvider = areaWithPermissionPathProvider;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		[UnitOfWork, System.Web.Http.HttpGet, System.Web.Http.Route("api/Global/Application/Areas")]
		public virtual IEnumerable<object> GetAreas()
		{
			return _areaWithPermissionPathProvider.GetWfmAreasWithPermissions()
				.Select(a => new { Name = a.Name(), a.InternalName, _links = a.Links.ToArray() }).ToArray();
		}

		[UnitOfWork, System.Web.Http.HttpGet, System.Web.Http.Route("api/Settings/SupportEmail")]
		public virtual IHttpActionResult GetSupportEmailSetting()
		{
			var emailSetting = _globalSettingDataRepository.FindValueByKey("SupportEmailSetting",
					new StringSetting());
			return Ok(emailSetting.StringValue);
		}
	}
}