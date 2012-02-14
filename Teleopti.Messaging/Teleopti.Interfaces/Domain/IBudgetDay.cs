namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// The budget for a particular date, planning group and scenario.
    /// </summary>
    public interface IBudgetDay : IAggregateRoot
    {
        /// <summary>
        /// Gets the date budgeted for.
        /// </summary>
        /// <value>The day.</value>
        DateOnly Day { get; }

        /// <summary>
        /// Gets the scenario that the budget belongs to.
        /// </summary>
        /// <value>The scenario.</value>
        IScenario Scenario { get; }

        /// <summary>
        /// Gets the budget group that the day belongs to
        /// </summary>
        /// <value>The budget group.</value>
        IBudgetGroup BudgetGroup { get; }

        ///<summary>
        /// The hours a full time equivalent works
        ///</summary>
        double FulltimeEquivalentHours { get; set; }

        ///<summary>
        /// The number of employed staff
        ///</summary>
        double? StaffEmployed { get; set; }

        ///<summary>
        /// The attrition rate on a yearly basis
        ///</summary>
        Percent AttritionRate { get; set; }

        ///<summary>
        /// Number of FTE recruiments for this day
        ///</summary>
        double Recruitment { get; set; }

        ///<summary>
        /// Number of FTE contractors(in hours) are hired for this day
        ///</summary>
        double Contractors { get; set; }

        ///<summary>
        /// Number of days off per week
        ///</summary>
        double DaysOffPerWeek { get; set; }

        ///<summary>
        /// Number of hours of overtime budgeted for this day
        ///</summary>
        double OvertimeHours { get; set; }

        ///<summary>
        /// Number of hours of students work budgeted for this day
        ///</summary>
        double StudentHours { get; set; }

        ///<summary>
        /// The forecasted hours of total workload for this day
        ///</summary>
        double ForecastedHours { get; set; }

        ///<summary>
        /// This days values for custom shrinkages
        ///</summary>
        ICustomShrinkageWrapper CustomShrinkages { get; }

        ///<summary>
        /// This days values for custom shrinkages
        ///</summary>
        ICustomEfficiencyShrinkageWrapper CustomEfficiencyShrinkages { get; }

        /// <summary>
        /// Indicates if the day is closed or not
        /// </summary>
        bool IsClosed { get; set; }

        /// <summary>
        /// Indicates the percentage of auto-grant leave 
        /// </summary>
        Percent AbsenceThreshold { get; set; }

        /// <summary>
        /// Number of extra FTEs can request leave
        /// </summary>
        double? AbsenceExtra { get; set; }

        /// <summary>
        /// Number of auto-grant leave regardless of budgeted FTEs
        /// </summary>
        double? AbsenceOverride { get; set; }

        /// <summary>
        /// Total budget allowance for leave
        /// </summary>
        double TotalAllowance { get; set; }

        /// <summary>
        /// Budget allowance for leave
        /// </summary>
        double Allowance { get; set; }

        /// <summary>
        /// Calculates the specified calculator.
        /// </summary>
        BudgetCalculationResult Calculate(IBudgetCalculator calculator);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 10/15/2010
    /// </remarks>
    public interface IBudgetCalculator
    {
        /// <summary>
        /// Calculates the specified budget day.
        /// </summary>
        BudgetCalculationResult Calculate(IBudgetDay budgetDay);
    }
}