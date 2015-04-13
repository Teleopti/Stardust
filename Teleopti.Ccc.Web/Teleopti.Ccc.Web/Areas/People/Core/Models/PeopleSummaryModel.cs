using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.People.Core.ViewModels
{
	public class PeopleSummaryModel
	{
	public IList<Interfaces.Domain.IPerson> People { get; set; }

		public int TotalPages { get; set; }

		public IList<Interfaces.Domain.IOptionalColumn> OptionalColumns { get; set; }
	}
}