using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class OvertimeRequestViewModel : RequestViewModel
	{
		public string OvertimeTypeDescription { get; set; }

		public IEnumerable<string> BrokenRules { get; set; }

	}
}