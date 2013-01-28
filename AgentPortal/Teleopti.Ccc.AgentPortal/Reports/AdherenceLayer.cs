using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Reports
{
    public class AdherenceLayer
    {
        private TimePeriod _period;
        private double _adherence;
    	private DateTime _calendarDate;
    	private DateTime _shiftBelongsToDate;

        public AdherenceLayer(TimePeriod period, double adherence, DateTime calendarDate, DateTime shiftBelongsToDate)
        {
            _period = period;
            _adherence = adherence;
        	_calendarDate = calendarDate;
        	_shiftBelongsToDate = shiftBelongsToDate;
        }

    	public DateTime CalendarDate
    	{
    		get { return _calendarDate; }
			set { _calendarDate = value; }
    	}

    	public DateTime ShiftBelongsToDate
    	{
    		get { return _shiftBelongsToDate; }
			set { _shiftBelongsToDate = value; }
    	}

    	public double Adherence
        {
            get { return _adherence; }
            set { _adherence = value; }
        }

        public TimePeriod Period
        {
            get { return _period; }
            set { _period = value; }
        }
    }
}
