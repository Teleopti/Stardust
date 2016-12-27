using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IBudgetAbsenceAllowanceDetail
	{
		double Allowance { get; set; }
		double TotalAllowance { get; set; }
		IDictionary<IAbsence, double> UsedAbsencesDictionary { get; set; }
		double UsedTotalAbsences { get; set; }
		double AbsoluteDifference { get; set; }
		Percent RelativeDifference { get; set; }
		DateOnly Date { get; set; }
		double TotalHeadCounts { get; set; }
	}
}
