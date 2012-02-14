using System;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public interface IBudgetGroupMonthDetailModel : INotifyPropertyChanged
    {
        IBudgetDay BudgetDay { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
        string Date { get; set; }
        string Month { get; set; }
        string Year { get; set; }
        double FulltimeEquivalentHours { get; set; }
        double? StaffEmployed { get; set; }
        Percent AttritionRate { get; set; }
        double Recruitment { get; set; }
        double GrossStaff { get; set; }
        double Contractors { get; set; }
        double DaysOffPerWeek { get; set; }
        double NetStaff { get; set; }
        double NetStaffFcAdj { get; set; }
        int OvertimeHours { get; set; }
        int StudentsHours { get; set; }
        double BudgetedStaff { get; set; }
        double ForecastedHours { get; set; }
        double ForecastedStaff { get; set; }
        double Difference { get; set; }
        Percent DifferencePercent { get; set; }
        void UpdateBudgetDay();
        void Recalculate(IBudgetCalculator calculator);
        event EventHandler<CustomEventArgs<IBudgetGroupDayDetailModel>> Invalidate;
        Percent GetShrinkage(ICustomShrinkage customShrinkage);
        void SetShrinkage(ICustomShrinkage customShrinkage, Percent percent);
        Percent GetEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage);
        void SetEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage, Percent percent);
        void DisablePropertyChangedInvocation();
        void EnablePropertyChangedInvocation();
    }
}