using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public static class NetStaffFcAdjustSurplusDistributor
	{
		public static double Distribute(IBudgetDay budgetDay, double currentNetStaffFcAdj, double surplusToDistribute,
		                                double maxStaff)

	{
		if (currentNetStaffFcAdj + surplusToDistribute > maxStaff)
		{
			currentNetStaffFcAdj = maxStaff;
			budgetDay.NetStaffFcAdjustedSurplus = currentNetStaffFcAdj - maxStaff;
		}
		else
		{
			currentNetStaffFcAdj += surplusToDistribute;
			budgetDay.NetStaffFcAdjustedSurplus = null;
		}
		return currentNetStaffFcAdj;
	}
	}
}
