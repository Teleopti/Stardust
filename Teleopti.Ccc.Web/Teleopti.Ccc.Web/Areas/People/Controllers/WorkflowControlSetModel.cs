using System;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class WorkflowControlSetModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public bool IsDeleted { get; set; }
	}
}