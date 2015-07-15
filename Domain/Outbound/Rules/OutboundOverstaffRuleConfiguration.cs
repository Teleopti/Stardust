using System;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    class OutboundOverstaffRuleConfiguration : OutboundRuleConfiguration
    {
        public override Type GetTypeOfRule()
        {
            return typeof (OutboundOverstaffRule);
        }

        public int Threshold { get; set; }
        public ThresholdType ThresholdType { get; set; }
    }
}
