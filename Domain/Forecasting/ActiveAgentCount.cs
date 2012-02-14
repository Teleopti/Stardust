using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class ActiveAgentCount : IActiveAgentCount
    {
        public DateTime Interval { get; set; }
        public int ActiveAgents { get; set; }
    }
}