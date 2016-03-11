using System;
using System.Linq;
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
			var bridgeListForAcdLoginId = _personPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(bridgeAcdLoginPerson.AcdLoginId);
			if (bridgeListForAcdLoginId.Any(a => a.PersonId == -1))
			{
				_personPeriodRepository.DeleteBridgeAcdLoginPerson(bridgeAcdLoginPerson.AcdLoginId, -1);
				_personPeriodRepository.AddBridgeAcdLoginPerson(bridgeAcdLoginPerson);
			}
			else if (!bridgeListForAcdLoginId.Any(a => a.AcdLoginId == bridgeAcdLoginPerson.AcdLoginId && a.PersonId == bridgeAcdLoginPerson.PersonId))
			{
				_personPeriodRepository.AddBridgeAcdLoginPerson(bridgeAcdLoginPerson);
			}
		}

		public void DeleteAcdLoginPerson(AnalyticsBridgeAcdLoginPerson bridgeAcdLoginPerson)
		{
			var bridgeListForAcdLoginId = _personPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(bridgeAcdLoginPerson.AcdLoginId);
			if (bridgeListForAcdLoginId.Count == 1 &&
				bridgeListForAcdLoginId.First().AcdLoginId == bridgeAcdLoginPerson.AcdLoginId &&
				bridgeListForAcdLoginId.First().PersonId == bridgeAcdLoginPerson.PersonId)
			{
				_personPeriodRepository.DeleteBridgeAcdLoginPerson(bridgeAcdLoginPerson.AcdLoginId, bridgeAcdLoginPerson.PersonId);
				_personPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson
				{
					AcdLoginId = bridgeAcdLoginPerson.AcdLoginId,
					PersonId = -1,
					TeamId = -1,
					BusinessUnitId = -1,
					DatasourceId = -1,
					DatasourceUpdateDate = new DateTime(2059, 12, 31)
				});
			}
			else
			{
				_personPeriodRepository.DeleteBridgeAcdLoginPerson(bridgeAcdLoginPerson.AcdLoginId, bridgeAcdLoginPerson.PersonId);
			}
		}
	}
}
