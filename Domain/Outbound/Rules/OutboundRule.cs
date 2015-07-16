using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{


    abstract public class OutboundRule
    {       
        public abstract void Configure(OutboundRuleConfiguration inputConfiguration);
        public abstract IEnumerable<OutboundRuleResponse> Validate(IOutboundCampaign campaign);
    }   
}
