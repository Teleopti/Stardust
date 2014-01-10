using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.UndoRedo
{
	public class BatchMemento : IMemento
	{
		public BatchMemento(string description)
		{
			MementoCollection = new List<IMemento>();
			Description = description;
		}

		public IList<IMemento> MementoCollection { get; private set; }

		public string Description { get; private set; }

		public IMemento Restore()
		{
			var inverse = new BatchMemento(Description);
			for (var i = MementoCollection.Count - 1; i >= 0; i--)
				inverse.MementoCollection.Add(MementoCollection[i].Restore());
			return inverse;
		}
	}
}
