using System;

namespace Stardust.Node.Interfaces
{
	public interface IJobToDo
	{
		Guid Id { get; set; }
		string Name { get; set; }
		string Serialized { get; set; }
		string Type { get; set; }
	}
}