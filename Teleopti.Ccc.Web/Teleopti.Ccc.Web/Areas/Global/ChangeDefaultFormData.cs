using System;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ChangeDefaultFormData
	{
		public Guid CurrentDefaultId { get; set; }
		public Guid? PreDefaultId { get; set; }
	}
}