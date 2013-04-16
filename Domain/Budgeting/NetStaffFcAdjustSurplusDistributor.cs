using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public static class NetStaffFcAdjustSurplusDistributor
	{
		public static double Distribute(IBudgetDay budgetDay, double currentNetStaffFcAdj, double surplusToDistribute,
		                                double maxStaff)
		{
			var sumNetStaffFcAdj = currentNetStaffFcAdj + surplusToDistribute;
			if (sumNetStaffFcAdj > maxStaff)
			{
				budgetDay.NetStaffFcAdjustedSurplus = sumNetStaffFcAdj - maxStaff;
				return maxStaff;
			}
			budgetDay.NetStaffFcAdjustedSurplus = 0;
			return sumNetStaffFcAdj;
		}
	}
}
