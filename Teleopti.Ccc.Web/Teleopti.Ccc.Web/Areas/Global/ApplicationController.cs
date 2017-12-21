using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

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

		[UnitOfWork, HttpGet, Route("api/Global/Application/WfmAreasWithPermission")]
		public virtual IEnumerable<object> GetWfmAreasWithPermission()
		{
			return _areaWithPermissionPathProvider.GetWfmAreasWithPermissions()
				.Select(a => new { Name = a.Name(), a.InternalName, _links = a.Links.ToArray() }).ToArray();
		}

		[UnitOfWork, HttpGet, Route("api/Global/Application/WfmAreasList")]
		public virtual IEnumerable<object> GetWfmAreasList()
		{
			return _areaWithPermissionPathProvider.GetWfmAreasList()
				.Select(a => new { Name = a.Name(), a.InternalName, _links = a.Links.ToArray() }).ToArray();
		}

		[UnitOfWork, HttpGet, Route("api/Settings/SupportEmail")]
		public virtual IHttpActionResult GetSupportEmailSetting()
		{
			var emailSetting = _globalSettingDataRepository.FindValueByKey("SupportEmailSetting",
					new StringSetting());
			return Ok(emailSetting.StringValue);
		}
	}
}