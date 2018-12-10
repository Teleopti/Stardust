using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class BudgetAbsenceAllowanceDetailViewModel
	{
		public double FullAllowance { get; set; }
		public double ShrinkedAllowance { get; set; }
		public IDictionary<string, double> UsedAbsencesDictionary { get; set; }
		public double UsedTotalAbsences { get; set; }
		public double AbsoluteDifference { get; set; }
		public double? RelativeDifference { get; set; }
		public DateOnly Date { get; set; }
		public double TotalHeadCounts { get; set; }
		public bool IsWeekend { get; set; }
	}

	public class BudgetGroupViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}