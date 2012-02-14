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

        public AdherenceLayer(TimePeriod period, double adherence)
        {
            _period = period;
            _adherence = adherence;
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
