using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    public class OutboundUnderSLARuleConfiguration : OutboundRuleConfiguration
    {
        public override Type GetTypeOfRule()
        {
            return typeof(OutboundUnderSLARule);
        }

        public double Threshold { get; set; }
        public ThresholdType ThresholdType { get; set; }
    }
}
