using System;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.People.Core.Aspects;
using Teleopti.Ccc.Web.Areas.Staffing.Controllers;

namespace Teleopti.Ccc.Web.Areas.Staffing
{
	public class StaffingAuditContextAction : IHandleContextAction<ClearBpoActionObj>, IHandleContextAction<ImportBpoActionObj>
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