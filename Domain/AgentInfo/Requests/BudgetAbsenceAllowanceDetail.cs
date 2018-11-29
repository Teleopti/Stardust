using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class BudgetAbsenceAllowanceDetail : IBudgetAbsenceAllowanceDetail
	{
		public double ShrinkedAllowance { get; set; }
		public double FullAllowance { get; set; }
		public IDictionary<IAbsence, double> UsedAbsencesDictionary { get; set; }
		public double UsedTotalAbsences { get; set; }
		public double AbsoluteDifference { get; set; }
		public Percent RelativeDifference { get; set; }
		public DateOnly Date { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
			MessageId = "TotalHeadCounts")]
		public double TotalHeadCounts { get; set; }
	}
}