using System;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class FavoriteSearchFormData
	{
		public Guid? Id { get; set; }
		public string Name { get; set;  }
		public string SearchTerm { get; set; }
		public Guid[] TeamIds { get; set; }
		public bool IsDefault { get; set; }
	}
}