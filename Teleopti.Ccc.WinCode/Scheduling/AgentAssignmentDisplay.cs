using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Describes how an PersonAssignment should be displayed in period view.
    /// Zoë
    /// </summary>
    public class AgentAssignmentDisplay
    {
        private DisplayMode _displayMode;
        private readonly PersonAssignment _personAssignment;
        private readonly DateTimePeriod _date;

        /// <summary>
        /// Creates a new instance of AgentAssignmentDisplay
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="date"></param>
        public AgentAssignmentDisplay(PersonAssignment assignment, DateTimePeriod date)
        {
            _personAssignment = assignment;
            _date = date;
            SetDisplayMode();
        }

        /// <summary>
        /// Gets the DisplayMode
        /// </summary>
        public DisplayMode DisplayMode
        {
            get { return _displayMode; }
        }

        /// <summary>
        /// Gets the AgentAssignment
        /// </summary>
        public PersonAssignment PersonAssignment
        {
            get { return _personAssignment; }
        }

        /// <summary>
        /// Sets the DisplayMode. (How the object should be displayed in the period viw grid)
        /// </summary>
        private void SetDisplayMode()
        {
            DateTimePeriod period = _personAssignment.Period();
            if (period.LocalStartDateTime >= _date.LocalStartDateTime && period.LocalEndDateTime < _date.LocalEndDateTime)
            {
                _displayMode = DisplayMode.BeginsAndEndsToday;
            }
            else if (period.LocalStartDateTime < _date.LocalStartDateTime && period.LocalEndDateTime <= _date.LocalEndDateTime)
            {
                _displayMode = DisplayMode.EndsToday;
            }
            else if (period.LocalStartDateTime >= _date.LocalStartDateTime && period.LocalEndDateTime > _date.LocalEndDateTime)
            {
                _displayMode = DisplayMode.BeginsToday;
            }
        }
    }
}