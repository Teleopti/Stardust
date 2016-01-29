using System;

namespace Stardust.Manager.Interfaces
{
	public interface INodeManager
	{
		void AddIfNeeded(Uri nodeUrl);
		void FreeJobIfAssingedToNode(Uri url);
	}
}