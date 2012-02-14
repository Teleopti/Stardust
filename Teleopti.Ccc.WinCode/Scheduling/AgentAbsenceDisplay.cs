using System.Drawing;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Describes how an PersonAbsence should be displayed in period view.
    /// Zoë
    /// </summary>
    public class AgentAbsenceDisplay
    {
        private DisplayMode _displayMode;
        private  AbsenceLayer _absenceLayer;
        private readonly PersonAbsence _personAbsence;
        private readonly DateTimePeriod _dateTimePeriod;


        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAbsenceDisplay"/> class.
        /// </summary>
        /// <param name="personAbsence">The agent absence.</param>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2007-11-22
        /// </remarks>
        public AgentAbsenceDisplay(PersonAbsence personAbsence, DateTimePeriod dateTimePeriod)
        {
            _personAbsence = personAbsence;
            _dateTimePeriod = dateTimePeriod;
            SetDisplayMode();
        }

        /// <summary>
        /// Gets the AbsenceLayer
        /// </summary>
        public AbsenceLayer AbsenceLayer
        {
            get { return _absenceLayer; }
        }

        /// <summary>
        /// Gets the DisplayMode
        /// </summary>
        public DisplayMode DisplayMode
        {
            get { return _displayMode; }
        }

        /// <summary>
        /// Gets the DisplayColor
        /// </summary>
        public Color DisplayColor
        {
            get { return _absenceLayer.Payload.DisplayColor; }
        }

        /// <summary>
        /// Gets the agent absence.
        /// </summary>
        /// <value>The agent absence.</value>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2007-11-22
        /// </remarks>
        public PersonAbsence PersonAbsence
        {
            get { return _personAbsence; }
        }

        /// <summary>
        /// Sets the DisplayMode. (How the object should be displayed in the period viw grid)
        /// </summary>
        public void SetDisplayMode()
        {
            DateTimePeriod period = _personAbsence.Layer.Period;
            _absenceLayer = _personAbsence.Layer;
            if (_absenceLayer.Period.ContainsPart(_dateTimePeriod))
            {
                if (_absenceLayer.IsFullDayAbsence)
                {
                    _displayMode = DisplayMode.WholeDay;
                }
                else if (period.LocalStartDateTime >= _dateTimePeriod.LocalStartDateTime && period.LocalEndDateTime <= _dateTimePeriod.LocalEndDateTime)
                {
                    _displayMode = DisplayMode.BeginsAndEndsToday;
                }
                else if (period.LocalStartDateTime < _dateTimePeriod.LocalStartDateTime && period.LocalEndDateTime <= _dateTimePeriod.LocalEndDateTime)
                {
                    _displayMode = DisplayMode.EndsToday;
                }
                else if (period.LocalStartDateTime >= _dateTimePeriod.LocalStartDateTime && period.LocalEndDateTime > _dateTimePeriod.LocalEndDateTime)
                {
                    _displayMode = DisplayMode.BeginsToday;
                }
                else
                {
                    _displayMode = DisplayMode.WholeDay;
                }
            }
        }
    }
}