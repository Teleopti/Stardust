using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleData
	{
		public DateTime Date { get; set; }
		public IPerson Person { get; set; }
	}
}