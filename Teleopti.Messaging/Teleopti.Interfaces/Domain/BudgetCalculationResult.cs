namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by:  peterwe
    /// Created date: 10/15/2010
    /// </remarks>
    public class BudgetCalculationResult 
    {
        /// <summary>
        /// Gets or sets the gross staff.
        /// </summary>
        /// <value>The gross staff.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public double GrossStaff { get; set; }
        /// <summary>
        /// Gets or sets the net staff.
        /// </summary>
        /// <value>The net staff.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public double NetStaff { get; set; }
        /// <summary>
        /// Gets or sets the net staff fc adj.
        /// </summary>
        /// <value>The net staff fc adj.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public double NetStaffFcAdj { get; set; }
        /// <summary>
        /// Gets or sets the budgeted staff.
        /// </summary>
        /// <value>The budgeted staff.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public double BudgetedStaff { get; set; }
        /// <summary>
        /// Gets or sets the forecasted staff.
        /// </summary>
        /// <value>The forecasted staff.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public double ForecastedStaff { get; set; }
        /// <summary>
        /// Gets or sets the difference.
        /// </summary>
        /// <value>The difference.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public double Difference { get; set; }
        /// <summary>
        /// Gets or sets the difference percent.
        /// </summary>
        /// <value>The difference percent.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public Percent DifferencePercent { get; set; }
         /// <summary>
        /// Gets or sets the budgeted leave for absence.
        /// </summary>
        /// <value>The budgeted leave.</value>
        public double BudgetedLeave { get; set; }
        /// <summary>
        /// Gets or sets the budgeted surplus for absence.
        /// </summary>
        /// <value>The budgeted surplus.</value>
        public double BudgetedSurplus { get; set; }
        /// <summary>
        /// Gets or sets the allowance for absence.
        /// </summary>
        /// <value>The allowance.</value>
        public double Allowance { get; set; }
        /// <summary>
        /// Gets or sets the total allowance for absence.
        /// </summary>
        /// <value>The allowance.</value>
        public double TotalAllowance { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetCalculationResult"/> class.
        /// </summary>
        /// <param name="grossStaff">The gross staff.</param>
        /// <param name="netStaff">The net staff.</param>
        /// <param name="netStaffFcAdj">The net staff fc adj.</param>
        /// <param name="budgetedStaff">The budgeted staff.</param>
        /// <param name="forecastedStaff">The forecasted staff.</param>
        /// <param name="difference">The difference.</param>
        /// <param name="differencePercent">The difference percent.</param>
        /// <param name="budgetedLeave">The budgeted leave.</param>
        /// <param name="budgetedSurplus">The budgeted surplus.</param>
        /// <param name="allowance">The allowance.</param>
        /// <param name="totalAllowance">The total allowance</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public BudgetCalculationResult(double grossStaff,
            double netStaff, double netStaffFcAdj,
            double budgetedStaff, double forecastedStaff, 
            double difference, Percent differencePercent, double budgetedLeave, double budgetedSurplus, double allowance, double totalAllowance)
        {
            GrossStaff = grossStaff;
            NetStaff = netStaff;
            NetStaffFcAdj = netStaffFcAdj;
            BudgetedStaff = budgetedStaff;
            ForecastedStaff = forecastedStaff;
            Difference = difference;
            DifferencePercent = differencePercent;
            BudgetedLeave = budgetedLeave;
            BudgetedSurplus = budgetedSurplus;
            Allowance = allowance;
            TotalAllowance = totalAllowance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetCalculationResult"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2010
        /// </remarks>
        public BudgetCalculationResult()
        {
        }
    }
}
