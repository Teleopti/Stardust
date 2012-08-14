using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Test
{
	public class TestMessageViewModel
	{
		public TestMessageViewModel()
		{
			ListItems = new string[] {};
		}

		public string Title { get; set; }
		public string Message { get; set; }
		public IEnumerable<string> ListItems { get; set; }
	}
}