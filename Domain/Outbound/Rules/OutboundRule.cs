using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    public enum ThresholdType
    {
        Absolute,
        Relative
    }

    public class OutboundRuleResponse
    {
        public bool IsValid { get; set; }
        public Type TypeOfRule { get; set; }
        public IOutboundCampaign Campaign { get; set; }
        public int? Threshold { get; set; }
        public ThresholdType ThresholdType { get; set; }
        public int? TargetValue { get; set; }
        public DateTime Date { get; set; }
    }

    abstract public class OutboundRule
    {       
        public abstract void Configure(OutboundRuleConfiguration inputConfiguration);
        public abstract IEnumerable<OutboundRuleResponse> Validate(IOutboundCampaign campaign);
    }   
}
