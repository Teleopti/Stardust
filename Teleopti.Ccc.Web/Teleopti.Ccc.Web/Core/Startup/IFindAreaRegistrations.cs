using System.Collections.Generic;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public interface IFindAreaRegistrations
	{
		IEnumerable<AreaRegistration> AreaRegistrations();
	}
}