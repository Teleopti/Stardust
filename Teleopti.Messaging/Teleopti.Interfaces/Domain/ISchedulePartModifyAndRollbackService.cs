using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Used for modifying and rolling back schedule changes.
    ///</summary>
    public interface ISchedulePartModifyAndRollbackService
    {
        /// <summary>
        /// Modifies the specified schedule part and saves the previous schedule in an internal list.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-10-29
        /// </remarks>
        void Modify(IScheduleDay schedulePart);

        /// <summary>
        /// Modifies the specified schedule part.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="scheduleTagSetter">The schedule tag setter.</param>
        void Modify(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter);

        /// <summary>
        /// Performs a rollback to the state of when this class was instatiated.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-10-29
        /// </remarks>
        void Rollback();

        /// <summary>
        /// Rollbacks the last modify operation.
        /// </summary>
        void RollbackLast();

        /// <summary>
        /// Gets the length of the stack.
        /// </summary>
        /// <value>The length of the stack.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-10-30
        /// </remarks>
        int StackLength { get; }

        /// <summary>
        /// Gets the modification collection.
        /// </summary>
        /// <value>The modification collection.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-10-30
        /// </remarks>
        ReadOnlyCollection<IScheduleDay> ModificationCollection { get; }

        /// <summary>
        /// Clears the modification collection.
        /// </summary>
        void ClearModificationCollection();
    }
}
