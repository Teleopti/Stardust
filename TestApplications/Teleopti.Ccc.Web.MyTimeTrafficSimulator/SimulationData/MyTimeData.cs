using System;

namespace Teleopti.Ccc.Web.MyTimeTrafficSimulator.SimulationData
{
    public class MyTimeData : SimulationDataBase
    {
        public Guid User { get; set; }
        public Guid AbsenseId { get; set; }
	    public string Username { get; set; }
	    public string Password { get; set; }
    }
}