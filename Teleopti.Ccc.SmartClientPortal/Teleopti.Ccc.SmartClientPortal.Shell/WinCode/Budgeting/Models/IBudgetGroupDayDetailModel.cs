using System;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
	public interface IBudgetGroupDayDetailModel : INotifyPropertyChanged
	{
		IBudgetDay BudgetDay { get; set; }
		DateDayModel Date { get; set; }
		string Week { get; set; }
		double FulltimeEquivalentHours { get; set; }
		double? StaffEmployed { get; set; }
		Percent AttritionRate { get; set; }
		double Recruitment { get; set; }
		double GrossStaff { get; set; }
		double Contractors { get; set; }
		double DaysOffPerWeek { get; set; }
		bool IsClosed { get; set; }
		double NetStaff { get; set; }
		double NetStaffFcAdj { get; set; }
		double OvertimeHours { get; set; }
		double StudentsHours { get; set; }
		double BudgetedStaff { get; set; }
		double ForecastedHours { get; set; }
		double ForecastedStaff { get; set; }
		double Difference { get; set; }
		Percent DifferencePercent { get; set; }
		Percent AbsenceThreshold { get; set; }
		double? AbsenceExtra { get; set; }
		double? AbsenceOverride { get; set; }
		double BudgetedLeave { get; set; }
		double BudgetedSurplus { get; set; }
		double ShrinkedAllowance { get; set; }
		double FullAllowance { get; set; }

		void UpdateBudgetDay();

		void Recalculate(IBudgetCalculator calculator);

		void RecalculateWithoutNetStaffForecastAdjustCalculator(IBudgetCalculator calculator, double netStaffFcAdj);

		event EventHandler<CustomEventArgs<IBudgetGroupDayDetailModel>> Invalidate;

		Percent GetShrinkage(ICustomShrinkage customShrinkage);

		void SetShrinkage(ICustomShrinkage customShrinkage, Percent percent);

		Percent GetEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage);

		void SetEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage, Percent percent);

		void DisablePropertyChangedInvocation();

		void EnablePropertyChangedInvocation();

		void TriggerRecalculation();
	}
}