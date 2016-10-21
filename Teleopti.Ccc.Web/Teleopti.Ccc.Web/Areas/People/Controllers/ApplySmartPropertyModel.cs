using System;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public struct ApplySmartPropertyModel
	{
		public Guid PersonId { get; set; }
		public Guid ApplyId { get; set; }
	}
}