using System.Collections.Generic;
using Teleopti.Ccc.Domain.Outbound.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
    public class CampaignRuleChecker 
    {
        private readonly OutboundRuleConfigurationProvider _outboundRuleConfigurationProvider;
        private readonly OutboundUnderSLARule _outboundUnderSlaRule;
        private readonly OutboundOverstaffRule _outboundOverstaffRule;

        public CampaignRuleChecker(OutboundRuleConfigurationProvider outboundRuleConfigurationProvider, OutboundUnderSLARule outboundUnderSlaRule, OutboundOverstaffRule outboundOverstaffRule)
        {
            _outboundRuleConfigurationProvider = outboundRuleConfigurationProvider;
            _outboundUnderSlaRule = outboundUnderSlaRule;
            _outboundOverstaffRule = outboundOverstaffRule;            
        }        

        public IEnumerable<OutboundRuleResponse> CheckCampaign(IOutboundCampaign campaign)
        {
            var response = new List<OutboundRuleResponse>();

            foreach (var rule in new List<OutboundRule>{_outboundOverstaffRule, _outboundUnderSlaRule})
            {
                var config = _outboundRuleConfigurationProvider.GetConfiguration(rule.GetType());
                rule.Configure(config);
                response.AddRange(rule.Validate(campaign));
            }
           
            return response;
        }       
    }
}
