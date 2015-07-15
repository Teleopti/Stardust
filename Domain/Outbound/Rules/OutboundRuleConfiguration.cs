using System;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    abstract public class OutboundRuleConfiguration
    {
        public abstract Type GetTypeOfRule();        
    }
}
