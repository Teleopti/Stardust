using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModel
	{
		public string Name { get; set; }
		public string Site { get; set; }
		public string Team { get; set; }
		public IEnumerable<object> Layers { get; set; }
	}
}