using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.UndoRedo
{
	public class Memento<T> : IMemento
	{
		private readonly T _state;
		private readonly IOriginator<T> _owner;

		public Memento(IOriginator<T> owner, T oldState)
		{
			InParameter.NotNull(nameof(owner), owner);
			InParameter.NotNull(nameof(oldState), oldState);
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