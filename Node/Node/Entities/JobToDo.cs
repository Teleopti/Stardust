using System;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Entities
{
	public class JobToDo : IJobToDo
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Serialized { get; set; }
		public string Type { get; set; }
	}
}