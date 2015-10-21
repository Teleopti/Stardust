using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.ViewModels
{
	public class PeopleSummaryModel
	{
		public IList<IPerson> People { get; set; }

		public int TotalPages { get; set; }

		public IList<IOptionalColumn> OptionalColumns { get; set; }
	}
}