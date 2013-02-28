using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceTemplateViewModel : Option
	{

		public Guid? PreferenceId { get; set; }

		public string EarliestStartTime { get; set; }
		public string LatestStartTime { get; set; }

		public string EarliestEndTime { get; set; }
		public string LatestEndTime { get; set; }


		public string MinimumWorkTime { get; set; }
		public string MaximumWorkTime { get; set; }

		public Guid? ActivityPreferenceId { get; set; }

		public string ActivityMinimumTime { get; set; }
		public string ActivityMaximumTime { get; set; }

		public string ActivityEarliestStartTime { get; set; }
		public string ActivityLatestStartTime { get; set; }

		public string ActivityEarliestEndTime { get; set; }
		public string ActivityLatestEndTime { get; set; }
	}
}