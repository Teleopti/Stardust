using Autofac;
using Stardust.Node.Workers;

namespace Stardust.Node.Interfaces
{
	public interface INodeStarter
	{
		void Stop();

		void Start(NodeConfiguration nodeConfiguration, IContainer container);
	}
}