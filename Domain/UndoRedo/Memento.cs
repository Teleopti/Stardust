using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.UndoRedo
{
	public class Memento<T> : IMemento
	{
		private readonly T _state;
		private readonly IOriginator<T> _owner;

		public Memento(IOriginator<T> owner, T oldState)
		{
			InParameter.NotNull("owner", owner);
			InParameter.NotNull("oldState", oldState);
			_owner = owner;
			_state = oldState;
		}

		public IMemento Restore()
		{
			var prevState = _owner.CreateMemento();
			_owner.Restore(_state);
			return prevState;
		}
	}
}