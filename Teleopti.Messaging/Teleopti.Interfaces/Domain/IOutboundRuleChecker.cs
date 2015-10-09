using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
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
        public double? Threshold { get; set; }
        public ThresholdType ThresholdType { get; set; }
        public double? TargetValue { get; set; }
        public DateTime Date { get; set; }
    }

    public interface IOutboundRuleChecker
    {
        IEnumerable<OutboundRuleResponse> CheckCampaign(IOutboundCampaign campaign);
    }
}
