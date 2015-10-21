using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public interface IAreaWithPermissionPathProvider
	{
		IEnumerable<AreaWithPermissionPath> GetWfmAreasWithPermissions();
		IEnumerable<object> GetAreasWithPermissions();
	}
}