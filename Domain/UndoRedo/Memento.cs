using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.UndoRedo
{
	public class Memento<T> : IMemento
	{
		private readonly T _state;
		private readonly IOriginator<T> _owner;

		public Memento(IOriginator<T> owner, T oldState, string description)
		{
			InParameter.NotNull("owner", owner);
			InParameter.NotNull("oldState", oldState);
			_owner = owner;
			_state = oldState;
			Description = description;
			Time = DateTime.Now;
		}

		public DateTime Time { get; private set; }

		public string Description { get; private set; }

		public IMemento Restore()
		{
			var prevState = _owner.CreateMemento();
			_owner.Restore(_state);
			return prevState;
		}
	}
}