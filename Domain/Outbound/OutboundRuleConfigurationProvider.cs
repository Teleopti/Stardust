using System;
using Teleopti.Ccc.Domain.Outbound.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class OutboundRuleConfigurationProvider : IOutboundRuleConfigurationProvider
    {
        public OutboundRuleConfiguration GetConfiguration(Type rule)
        {
            if (rule == typeof (OutboundOverstaffRule))
            {
                return new OutboundOverstaffRuleConfiguration
                {
                    Threshold = 60,
                    ThresholdType = ThresholdType.Absolute                    
                };
            }

            if (rule == typeof (OutboundUnderSLARule))
            {
                return new OutboundUnderSLARuleConfiguration
                {
                    Threshold = 60,
                    ThresholdType = ThresholdType.Absolute
                };
            }

            return null;
        }
    }
}
