using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetBusinessUnitId
	{
		Guid Get(string teamId);
	}
}