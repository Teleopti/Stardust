using Rhino.ServiceBus;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class RecalculateForecastOnSkillConsumer:  ConsumerOf<RecalculateForecastOnSkillMessage>
	{
		
		public void Consume(RecalculateForecastOnSkillMessage message)
		{
			throw new System.NotImplementedException();
		}
	}
}