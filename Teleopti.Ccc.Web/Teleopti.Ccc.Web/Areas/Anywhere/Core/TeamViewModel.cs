using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamViewModel
	{
		public string SiteName { get; set; }
		public IEnumerable<TeamData> Teams { get; set; }

		public TeamViewModel()
		{
			Teams = Enumerable.Empty<TeamData>();
		}
	}
}