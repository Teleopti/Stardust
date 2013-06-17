namespace Teleopti.Interfaces.Domain
{
	public interface IMoveLayerVertical
	{
		void MoveUp(IPersonAssignment personAssignment, ILayer<IActivity> layer);
		void MoveDown(IPersonAssignment personAssignment, ILayer<IActivity> layer);
	}
}