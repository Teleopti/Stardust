using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.UndoRedo
{
	public class BatchMemento : IMemento
	{
		private readonly IList<IMemento> _mementoCollection;

		public BatchMemento(string description)
		{
			_mementoCollection = new List<IMemento>();
			Description = description;
		}

		public string Description { get; private set; }

		public void AddMemento(IMemento memento)
		{
			_mementoCollection.Add(memento);
		}

		public bool IsEmpty()
		{
			return _mementoCollection.Count == 0;
		}

		public IMemento Restore()
		{
			var inverse = new BatchMemento(Description);
			for (var i = _mementoCollection.Count - 1; i >= 0; i--)
				inverse.AddMemento(_mementoCollection[i].Restore());
			return inverse;
		}
	}
}
