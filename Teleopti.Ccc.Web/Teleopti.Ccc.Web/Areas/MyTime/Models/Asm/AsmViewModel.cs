using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Asm
{
	public class AsmViewModel
	{
		public AsmViewModel()
		{
			Layers = new List<AsmLayer>();
		}

		public IList<AsmLayer> Layers { get; private set; }
	}
}