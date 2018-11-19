using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class GlobalAreaController : ApiController
	{
		private readonly AreaWithPermissionPathProvider _areaWithPermissionPathProvider;

		public GlobalAreaController(AreaWithPermissionPathProvider areaWithPermissionPathProvider)
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