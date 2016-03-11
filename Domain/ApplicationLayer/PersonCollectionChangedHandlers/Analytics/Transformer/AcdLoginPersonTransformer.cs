using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer
{
	public class AcdLoginPersonTransformer
	{
		private readonly IAnalyticsPersonPeriodRepository _personPeriodRepository;

		public AcdLoginPersonTransformer(IAnalyticsPersonPeriodRepository personPeriodRepository)
		{
			_personPeriodRepository = personPeriodRepository;
		}

		public void AddAcdLoginPerson(AnalyticsBridgeAcdLoginPerson bridgeAcdLoginPerson)
		{
			
		}

		public void DeleteAcdLoginPerson(AnalyticsBridgeAcdLoginPerson bridgeAcdLoginPerson)
		{
			
		}
	}
}
