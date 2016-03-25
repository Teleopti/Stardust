using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Core.Data
{
	public interface IActivityProvider
	{
		IEnumerable<ActivityViewModel> GetAllRequireSkill();
		IList<ActivityViewModel> GetAll();
	}
}