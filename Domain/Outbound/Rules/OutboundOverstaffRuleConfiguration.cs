using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    public class OutboundOverstaffRuleConfiguration : OutboundRuleConfiguration
    {
        public override Type GetTypeOfRule()
        {
            return typeof (OutboundOverstaffRule);
        }

        public double Threshold { get; set; }
        public ThresholdType ThresholdType { get; set; }
    }
}
