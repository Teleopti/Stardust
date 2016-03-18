using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	public class AnalyticsGroupPage
	{
		public int GroupPageId { get; set; }
		public Guid GroupPageCode { get; set; }
		public string GroupPageName { get; set; }
		public string GroupPageNameResourceKey { get; set; }
		public int GroupId { get; set; }
		public Guid GroupCode { get; set; }
		public string GroupName { get; set; }
		public bool GroupIsCustom { get; set; }
		public int BusinessUnitId { get; set; }
		public Guid BusinessUnitCode { get; set; }
		public string BusinessUnitName { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
	}
}