using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class PeopleSummaryModel
	{
		public IList<IPerson> People { get; set; }

		public int TotalPages { get; set; }

		public IList<IOptionalColumn> OptionalColumns { get; set; }
	}
}