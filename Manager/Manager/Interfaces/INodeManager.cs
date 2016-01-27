namespace Stardust.Manager.Interfaces
{
	public interface INodeManager
	{
		void AddIfNeeded(string nodeUrl);
		void FreeJobIfAssingedToNode(string url);
	}
}