using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.ViewModels
{
	public class PeopleSummaryModel
	{
		public IList<IPerson> People { get; set; }

		public int TotalPages { get; set; }

		public IList<IOptionalColumn> OptionalColumns { get; set; }
	}
}