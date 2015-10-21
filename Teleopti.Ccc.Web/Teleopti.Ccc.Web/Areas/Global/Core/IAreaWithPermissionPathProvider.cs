using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Global.Core
{
	public interface IAreaWithPermissionPathProvider
	{
		IEnumerable<AreaWithPermissionPath> GetWfmAreasWithPermissions();
		IEnumerable<object> GetAreasWithPermissions();
	}
}