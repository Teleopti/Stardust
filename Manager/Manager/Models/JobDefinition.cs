using System;

namespace Stardust.Manager.Models
{
	public class JobDefinition
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Serialized { get; set; }
		public string Type { get; set; }
		public string UserName { get; set; }
		public string AssignedNode { get; set; }
		public string JobProgress { get; set; }
		public string Status { get; set; }
	}
}