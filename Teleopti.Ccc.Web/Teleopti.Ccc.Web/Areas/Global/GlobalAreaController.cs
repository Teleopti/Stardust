using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class GlobalAreaController : ApiController
	{
		private readonly IAreaWithPermissionPathProvider _areaWithPermissionPathProvider;

		public GlobalAreaController(IAreaWithPermissionPathProvider areaWithPermissionPathProvider)
		{
			_areaWithPermissionPathProvider = areaWithPermissionPathProvider;
		}

		[UnitOfWork, HttpGet, Route("Global/Application/Areas")]
		public virtual IEnumerable<object> GetApplicationAreas()
		{
			return _areaWithPermissionPathProvider.GetAreasWithPermissions();
		}
	}
}