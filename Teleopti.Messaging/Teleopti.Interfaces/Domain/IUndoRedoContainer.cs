using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Container for undo/redo
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-11-11
    /// </remarks>
    public interface IUndoRedoContainer
    {
        /// <summary>
        /// Gets a value indicating whether this instance is undo redo.
        /// Useful to prevent the undo/redo action itself creates a new undo/redo item
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is undo redo; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-11
        /// </remarks>
        bool InUndoRedo { get; }

        /// <summary>
        /// Determines whether this instance can redo.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance can redo; otherwise, <c>false</c>.
        /// </returns>
        bool CanRedo();

        /// <summary>
        /// Determines whether this instance can undo.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance can undo; otherwise, <c>false</c>.
        /// </returns>
        bool CanUndo();

        /// <summary>
        /// Clears the undo and redo stack.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-13
        /// </remarks>
        void Clear();

        /// <summary>
        /// Does a Redo
        /// </summary>
        /// <returns>
        /// True if stack wasn't empty (redo has occurred),
        /// false if stack was empty (nothing has happened)
        /// </returns>
        bool Redo();

        /// <summary>
        /// Saves the state of the originator object and clears the redo stack.
        /// </summary>
        /// <param name="state">The state.</param>
        void SaveState<T>(IOriginator<T> state);

        /// <summary>
        /// Does an Undo.
        /// </summary>
        /// <returns>
        /// True if stack wasn't empty (undo has occurred),
        /// false if stack was empty (nothing has happened)
        /// </returns>
        bool Undo();

        /// <summary>
        /// Creates a batch of mementos.
        /// </summary>
        /// <param name="description">The description</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-12
        /// </remarks>
        void CreateBatch(string description);

        /// <summary>
        /// Commits the batch.
        /// Ex
        /// undoRedo.CreateBatch()
        /// undoRedo.SaveState[...];
        /// undoRedo.SaveState[...];
        /// undoRedo.CommitBatch();
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-13
        /// </remarks>
        void CommitBatch();

        /// <summary>
        /// Rollbacks the batch.
        /// Ex
        /// undoRedo.CreateBatch()
        /// undoRedo.SaveState[...];
        /// undoRedo.SaveState[...];
        /// undoRedo.RollbackBatch();
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-13
        /// </remarks>
        void RollbackBatch();

        /// <summary>
        /// Occurs when container has changed.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-19
        /// </remarks>
        event EventHandler ChangedHandler;

        /// <summary>
        /// Undoes all.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-19
        /// </remarks>
        void UndoAll();
    }
}