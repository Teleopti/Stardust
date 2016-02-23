using System;

namespace Stardust.Node.Interfaces
{
	public interface IJobProgress<T>
	{
		Progress<T> JobProgress { get; set; }
	}
}