using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class RequestFormViewModel
	{
		public IEnumerable<AbsenceTypeViewModel> AbsenceTypes { get; set; }
	}
}