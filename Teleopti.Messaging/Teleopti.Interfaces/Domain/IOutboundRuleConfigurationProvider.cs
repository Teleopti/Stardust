using System;

namespace Teleopti.Interfaces.Domain
{
	abstract public class OutboundRuleConfiguration
    {
        public abstract Type GetTypeOfRule();        
    }

	public interface IOutboundRuleConfigurationProvider
	{
		OutboundRuleConfiguration GetConfiguration(Type rule);
	}

}
