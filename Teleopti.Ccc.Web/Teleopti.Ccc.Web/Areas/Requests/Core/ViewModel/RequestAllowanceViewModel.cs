using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class BudgetAbsenceAllowanceViewModel
	{
		public DateOnly[] Dates { get; set; }

		public AllowanceDetailViewModel[] Details { get; set; }
	}

	public class AllowanceDetailViewModel
	{
		public string Name { get; set; }

		public double[] Values { get; set; }
	}

	public class BudgetGroupViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}