using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Asm
{
	public class AsmViewModel
	{
		public IEnumerable<AsmLayer> Layers { get; set; }
		public DateTime StartDateTime { get; set; }
		public IEnumerable<string> Hours { get; set; }
	}
}