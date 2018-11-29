using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.UndoRedo
{
	public class UndoRedoContainer : IUndoRedoContainer
	{
		private readonly Stack<IMemento> _redoStack;
		private readonly Stack<IMemento> _undoStack;
		private BatchMemento _batchMemento;

		public UndoRedoContainer()
		{
			_undoStack = new Stack<IMemento>();
			_redoStack = new Stack<IMemento>();
		}

		public event EventHandler ChangedHandler;

		public bool InUndoRedo { get; private set; }

		public bool CanRedo()
		{
			return _redoStack.Count > 0;
		}

		public bool CanUndo()
		{
			return _undoStack.Count > 0;
		}

		public void Clear()
		{
			_redoStack.Clear();
			_undoStack.Clear();
			fireChanged();
		}

		public void UndoAll()
		{
			while (_undoStack.Count > 0)
			{
				Undo();
			}
		}

		public void SaveState<T>(IOriginator<T> state)
		{
			InParameter.NotNull(nameof(state), state);
			if (InUndoRedo)
				throw new InvalidOperationException("Cannot save state from within undo or redo action");

			var memento = state.CreateMemento();
			if (_batchMemento == null)
			{
				saveStateInternal(memento);
				fireChanged();
			}
			else
			{
				_batchMemento.AddMemento(memento);
			}
		}

		public virtual bool Undo()
		{
			if (_batchMemento != null)
				throw new InvalidOperationException("Current batch memento '" + _batchMemento.Description + "' wasn't ended.");

			var undoStackHasValue = CanUndo();
			if (undoStackHasValue)
			{
				InUndoRedo = true;
				var graph = _undoStack.Pop();
				_redoStack.Push(graph.Restore());
				fireChanged();
				InUndoRedo = false;
			}
			return undoStackHasValue;
		}

		public virtual bool Redo()
		{
			if (_batchMemento != null)
				throw new InvalidOperationException("Current batch memento '" + _batchMemento.Description + "' wasn't ended.");

			var redoStackHasValue = CanRedo();
			if (redoStackHasValue)
			{
				InUndoRedo = true;
				var graph = _redoStack.Pop();
				_undoStack.Push(graph.Restore());
				fireChanged();
				InUndoRedo = false;
			}
			return redoStackHasValue;
		}

		public void CreateBatch(string description)
		{
			if (_batchMemento != null)
				throw new InvalidOperationException("Previous batch memento '" + _batchMemento.Description + "' wasn't ended.");

			_batchMemento = new BatchMemento(description);
		}


		public void CommitBatch()
		{
			if (_batchMemento == null)
				throw new InvalidOperationException("Ending a non-existing batch memento");

			if (!_batchMemento.IsEmpty())
			{
				saveStateInternal(_batchMemento);
				fireChanged();
			}
			_batchMemento = null;
		}

		public void RollbackBatch()
		{
			if (_batchMemento == null)
				throw new InvalidOperationException("Ending a non-existing batch memento");
			_batchMemento = null;
		}

		private void fireChanged()
		{
			ChangedHandler?.Invoke(this, new EventArgs());
		}

		private void saveStateInternal(IMemento memento)
		{
			_undoStack.Push(memento);
			_redoStack.Clear();
		}
	}
}