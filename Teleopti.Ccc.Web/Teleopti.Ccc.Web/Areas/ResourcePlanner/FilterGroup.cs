using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class FilterGroup
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public List<FilterItem> Items { get; set; }
	}
}