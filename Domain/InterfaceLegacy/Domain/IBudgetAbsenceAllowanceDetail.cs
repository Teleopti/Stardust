using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IBudgetAbsenceAllowanceDetail
	{
		double ShrinkedAllowance { get; set; }
		double FullAllowance { get; set; }
		IDictionary<IAbsence, double> UsedAbsencesDictionary { get; set; }
		double UsedTotalAbsences { get; set; }
		double AbsoluteDifference { get; set; }
		Percent RelativeDifference { get; set; }
		DateOnly Date { get; set; }
		double TotalHeadCounts { get; set; }
	}
}