using System;
using Teleopti.Ccc.Web.Areas.Global.Aspect;
using Teleopti.Ccc.Web.Areas.Staffing.Controllers;

namespace Teleopti.Ccc.Web.Areas.Staffing
{
	public class StaffingAuditContext : IHandleContextAction<ClearBpoActionObj>, IHandleContextAction<ImportBpoActionObj>
	{
		public void Handle(ClearBpoActionObj command)
		{
			throw new NotImplementedException();
		}

		public void Handle(ImportBpoActionObj command)
		{
			throw new NotImplementedException();
		}
	}
}