using System.Collections.Generic;
using System.Web.Http;
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

		[HttpGet, Route("Global/Application/Areas")]
		public IEnumerable<object> GetApplicationAreas()
		{
			return _areaWithPermissionPathProvider.GetAreasWithPermissions();
		}
	}
}