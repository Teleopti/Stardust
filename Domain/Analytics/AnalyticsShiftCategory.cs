using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsShiftCategory
	{
		public int ShiftCategoryId { get; set; }
		public Guid ShiftCategoryCode { get; set; }
		public string ShiftCategoryName { get; set; }
		public string ShiftCategoryShortname { get; set; }
		public int DisplayColor { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public bool IsDeleted { get; set; }
	}
}