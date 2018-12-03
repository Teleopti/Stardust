using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class DatePersons
	{
		public DateOnly Date { get; set; }
		public IEnumerable<IPerson> Persons { get; set; }
	}
}