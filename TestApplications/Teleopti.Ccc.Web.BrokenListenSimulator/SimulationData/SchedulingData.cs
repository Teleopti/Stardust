using System;
using Teleopti.Ccc.Web.TestApplicationsCommon;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData
{
    public class SchedulingData : SimulationDataBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}