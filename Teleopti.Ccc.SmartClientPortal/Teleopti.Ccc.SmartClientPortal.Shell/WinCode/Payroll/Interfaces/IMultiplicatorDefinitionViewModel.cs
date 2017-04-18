using System;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces
{
    public interface IMultiplicatorDefinitionViewModel : IGridViewBaseModel, IViewModel<MultiplicatorDefinition>
    {
        /// <summary>
        /// Gets or sets the type of the multiplicator definition.
        /// </summary>
        /// <value>The type of the multiplicator definition.</value>
        MultiplicatorDefinitionAdapter MultiplicatorDefinitionType { get; }

        /// <summary>
        /// Gets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        DayOfWeek? DayOfWeek { get; set; }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The end time.</value>
        TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The end time.</value>
        TimeSpan? EndTime { get; set; }

        DateTime? FromDate { get; set; }

        DateTime? ToDate { get; set; }
    }
}
