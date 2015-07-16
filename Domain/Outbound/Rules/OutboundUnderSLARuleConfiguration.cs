using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    class OutboundUnderSLARuleConfiguration : OutboundRuleConfiguration
    {
        public override Type GetTypeOfRule()
        {
            return typeof(OutboundUnderSLARule);
        }

        public int Threshold { get; set; }
        public ThresholdType ThresholdType { get; set; }
    }
}
