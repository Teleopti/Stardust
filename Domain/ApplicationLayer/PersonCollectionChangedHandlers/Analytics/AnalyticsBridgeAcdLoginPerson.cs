using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	public class AnalyticsBridgeAcdLoginPerson
	{
		public int AcdLoginId { get; set; }
		public int PersonId { get; set; }
		public int TeamId { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
	}
}
