using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{   
    public class CampaignRuleChecker : IOutboundRuleChecker
    {
		private readonly IOutboundRuleConfigurationProvider _outboundRuleConfigurationProvider;
        private readonly OutboundUnderSLARule _outboundUnderSlaRule;
        private readonly OutboundOverstaffRule _outboundOverstaffRule;

		public CampaignRuleChecker(IOutboundRuleConfigurationProvider outboundRuleConfigurationProvider, OutboundUnderSLARule outboundUnderSlaRule, OutboundOverstaffRule outboundOverstaffRule)
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
                var result = rule.Validate(campaign).ToList();
                if (result.Count > 0)
                {
                    response.Add(result.First());
                }              
            }           
            return response;
        }       
    }
}
