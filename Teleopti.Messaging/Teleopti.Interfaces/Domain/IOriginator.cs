namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for creating mementos.
    /// Is supposed to be put on suitable aggregate roots.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOriginator<T>
    {
        /// <summary>
        /// Restores the memento.
        /// Be aware if objects in this graph is supposed to
        /// be replaced or if the data should be placed on
        /// current aggregate instances (probably the latter in most cases)!
        /// </summary>
        /// <param name="previousState">State of the previous graph.</param>
        void Restore(T previousState);

        /// <summary>
        /// Creates the memento.
        /// </summary>
        /// <returns>
        /// The newly created memento.
        /// Needed to get the redo action work.
        /// </returns>
        IMemento CreateMemento();
    }
}