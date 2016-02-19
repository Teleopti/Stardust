using System;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData
{
    public class SimulationDataBase
    {
        public string BaseUrl { get; set; }
        public string DataSource { get; set; }
        public Guid BusinessUnit { get; set; }
        public Guid Scenario { get; set; }
        public string BusinessUnitName { get; internal set; }
    }
}