using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class GlobalAreaController : ApiController
	{
		private readonly IAreaWithPermissionPathProvider _areaWithPermissionPathProvider;
		private readonly IToggleManager _toggleManager;

		public GlobalAreaController(IAreaWithPermissionPathProvider areaWithPermissionPathProvider, IToggleManager toggleManager)
		{
			_areaWithPermissionPathProvider = areaWithPermissionPathProvider;
			_toggleManager = toggleManager;
		}

		[UnitOfWork, HttpGet, Route("Global/Application/Areas")]
		public IEnumerable<object> GetApplicationAreas()
		{
			return _toggleManager.IsEnabled(Toggles.MyTimeWeb_KeepUrlAfterLogon_34762)
				? _areaWithPermissionPathProvider.GetAreasWithPermissions()
				: new List<object>();
		}
	}
}