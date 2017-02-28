using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class InputModel
	{
		public DateTime Date { get; set; }
		public IEnumerable<Guid> PersonIdList { get; set; }
	}
}