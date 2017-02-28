using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class PeopleShiftBagCommandInput
	{
		public DateTime Date { get; set; }
		public List<ShiftBagUpdateCommandInputModel> People { get; set; }
	}
}