using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Models
{
	public class FecthPersonsInputModel
	{
		public DateTime Date { get; set; }
		public IEnumerable<Guid> PersonIdList { get; set; }
	}
}